using System.Text.Json;
using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Plans;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;

namespace GymFit.Application.Services;

public sealed class PlanService : IPlanService
{
    private readonly IPlanRepository _plans;
    private readonly IUserRepository _users;
    private readonly ITrainerProfileRepository _trainerProfiles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PlanService(
        IPlanRepository plans,
        IUserRepository users,
        ITrainerProfileRepository trainerProfiles,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _plans = plans;
        _users = users;
        _trainerProfiles = trainerProfiles;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PlanDto> CreateAsync(Guid userId, CreatePlanDto request, CancellationToken cancellationToken = default)
    {
        if (!await _users.ExistsAsync(userId, cancellationToken))
            throw new KeyNotFoundException("User was not found.");

        var content = request.Content?.Trim() ?? "{}";
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Plan content cannot be empty.");

        try
        {
            JsonDocument.Parse(content);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Plan content must be valid JSON.");
        }

        Guid? trainerId = null;
        if (request.TrainerId.HasValue)
        {
            trainerId = request.TrainerId.Value;
            var trainer = await _users.GetByIdAsync(trainerId.Value, cancellationToken);
            if (trainer is null)
                throw new KeyNotFoundException("Trainer user was not found.");

            if (trainer.Role != UserRole.Trainer && trainer.Role != UserRole.Admin)
                throw new InvalidOperationException("Assigned user is not a trainer.");

            var profile = await _trainerProfiles.GetByUserIdWithUserAsync(trainerId.Value, cancellationToken);
            if (profile is null || !profile.IsApproved)
                throw new InvalidOperationException("Trainer does not have an approved profile.");
        }

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TrainerId = trainerId,
            Type = request.Type,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _plans.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _plans.GetByIdAsync(plan.Id, cancellationToken)
                      ?? throw new InvalidOperationException("Plan was created but could not be loaded.");

        return _mapper.Map<PlanDto>(created);
    }

    public async Task<PagedResult<PlanDto>> ListForUserAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var (items, total) = await _plans.ListByUserIdPagedAsync(userId, page, pageSize, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<PlanDto>>(items);
        return PagedResult<PlanDto>.Create(dtos, total, page, pageSize);
    }

    public async Task<PlanDto> GetAsync(Guid planId, Guid requesterUserId, CancellationToken cancellationToken = default)
    {
        var plan = await _plans.GetByIdWithUsersAsync(planId, cancellationToken);
        if (plan is null)
            throw new KeyNotFoundException("Plan was not found.");

        var isOwner = plan.UserId == requesterUserId;
        var isAssignedTrainer = plan.TrainerId == requesterUserId;
        if (!isOwner && !isAssignedTrainer)
            throw new InvalidOperationException("You are not allowed to view this plan.");

        return _mapper.Map<PlanDto>(plan);
    }
}
