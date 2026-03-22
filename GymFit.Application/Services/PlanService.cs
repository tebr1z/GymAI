using System.Text.Json;
using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Plans;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Services;

public sealed class PlanService : IPlanService
{
    private readonly IPlanRepository _plans;
    private readonly IUserRepository _users;
    private readonly ITrainerProfileRepository _trainerProfiles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PlanService> _logger;

    public PlanService(
        IPlanRepository plans,
        IUserRepository users,
        ITrainerProfileRepository trainerProfiles,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PlanService> logger)
    {
        _plans = plans;
        _users = users;
        _trainerProfiles = trainerProfiles;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ServiceResult<PlanDto>> CreateAsync(
        Guid userId,
        CreatePlanDto request,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(CreateAsync), async () =>
        {
            if (request is null)
                return ServiceResult<PlanDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            if (userId == Guid.Empty)
                return ServiceResult<PlanDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (!await _users.ExistsAsync(userId, cancellationToken))
                return ServiceResult<PlanDto>.Fail("User was not found.", ServiceFailureKind.NotFound);

            var content = request.Content?.Trim() ?? "{}";
            if (string.IsNullOrWhiteSpace(content))
                return ServiceResult<PlanDto>.Fail("Plan content cannot be empty.", ServiceFailureKind.BadRequest);

            try
            {
                JsonDocument.Parse(content);
            }
            catch (JsonException)
            {
                return ServiceResult<PlanDto>.Fail("Plan content must be valid JSON.", ServiceFailureKind.BadRequest);
            }

            Guid? trainerId = null;
            if (request.TrainerId.HasValue)
            {
                trainerId = request.TrainerId.Value;
                if (trainerId.Value == Guid.Empty)
                    return ServiceResult<PlanDto>.Fail("Trainer id is invalid.", ServiceFailureKind.BadRequest);

                var trainer = await _users.GetByIdAsync(trainerId.Value, cancellationToken);
                if (trainer is null)
                    return ServiceResult<PlanDto>.Fail("Trainer user was not found.", ServiceFailureKind.NotFound);

                if (trainer.Role != UserRole.Trainer && trainer.Role != UserRole.Admin)
                {
                    return ServiceResult<PlanDto>.Fail(
                        "Assigned user is not a trainer.",
                        ServiceFailureKind.BadRequest);
                }

                var profile = await _trainerProfiles.GetByUserIdWithUserAsync(trainerId.Value, cancellationToken);
                if (profile is null || !profile.IsApproved)
                {
                    return ServiceResult<PlanDto>.Fail(
                        "Trainer does not have an approved profile.",
                        ServiceFailureKind.BadRequest);
                }
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

            var created = await _plans.GetByIdAsync(plan.Id, cancellationToken);
            if (created is null)
            {
                return ServiceResult<PlanDto>.Fail(
                    "Plan was created but could not be loaded.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult<PlanDto>.Ok(_mapper.Map<PlanDto>(created));
        });

    public Task<ServiceResult<PagedResult<PlanDto>>> ListForUserAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(ListForUserAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult<PagedResult<PlanDto>>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
            var (items, total) = await _plans.ListByUserIdPagedAsync(userId, page, pageSize, cancellationToken);
            var dtos = _mapper.Map<IReadOnlyList<PlanDto>>(items);
            return ServiceResult<PagedResult<PlanDto>>.Ok(PagedResult<PlanDto>.Create(dtos, total, page, pageSize));
        });

    public Task<ServiceResult<PlanDto>> GetAsync(
        Guid planId,
        Guid requesterUserId,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetAsync), async () =>
        {
            if (planId == Guid.Empty || requesterUserId == Guid.Empty)
                return ServiceResult<PlanDto>.Fail("Invalid id.", ServiceFailureKind.BadRequest);

            var plan = await _plans.GetByIdWithUsersAsync(planId, cancellationToken);
            if (plan is null)
                return ServiceResult<PlanDto>.Fail("Plan was not found.", ServiceFailureKind.NotFound);

            var isOwner = plan.UserId == requesterUserId;
            var isAssignedTrainer = plan.TrainerId == requesterUserId;
            if (!isOwner && !isAssignedTrainer)
            {
                return ServiceResult<PlanDto>.Fail(
                    "You are not allowed to view this plan.",
                    ServiceFailureKind.Forbidden);
            }

            return ServiceResult<PlanDto>.Ok(_mapper.Map<PlanDto>(plan));
        });
}
