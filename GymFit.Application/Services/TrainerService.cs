using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Trainers;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;

namespace GymFit.Application.Services;

public sealed class TrainerService : ITrainerService
{
    private readonly ITrainerProfileRepository _trainerProfiles;
    private readonly ITrainerOrderRepository _orders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TrainerService(
        ITrainerProfileRepository trainerProfiles,
        ITrainerOrderRepository orders,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _trainerProfiles = trainerProfiles;
        _orders = orders;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<TrainerSummaryDto>> ListMarketplaceAsync(
        TrainerMarketplaceQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);

        var (profiles, total) = await _trainerProfiles.ListApprovedMarketplaceAsync(
            page,
            pageSize,
            query.MinRating,
            query.MaxPricePerMonth,
            cancellationToken);

        var items = profiles.Select(MapSummary).ToList();

        return PagedResult<TrainerSummaryDto>.Create(items, total, page, pageSize);
    }

    public async Task<TrainerDetailDto> GetTrainerAsync(Guid trainerProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _trainerProfiles.GetByIdWithUserAsync(trainerProfileId, cancellationToken);
        if (profile is null)
            throw new KeyNotFoundException("Trainer profile was not found.");

        if (!profile.IsApproved)
            throw new InvalidOperationException("This trainer profile is not available in the marketplace.");

        return MapDetail(profile);
    }

    public async Task<TrainerOrderDto> BookTrainerAsync(
        Guid clientUserId,
        BookTrainerDto request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _trainerProfiles.GetByIdWithUserAsync(request.TrainerProfileId, cancellationToken);
        if (profile is null)
            throw new KeyNotFoundException("Trainer profile was not found.");

        if (!profile.IsApproved)
            throw new InvalidOperationException("This trainer is not approved for bookings.");

        if (profile.UserId == clientUserId)
            throw new InvalidOperationException("You cannot book yourself as a trainer.");

        if (profile.User.Role != UserRole.Trainer && profile.User.Role != UserRole.Admin)
            throw new InvalidOperationException("The selected profile is not a trainer account.");

        if (profile.PricePerMonth < 0)
            throw new InvalidOperationException("Trainer pricing is invalid.");

        if (request.ExpectedPrice.HasValue && request.ExpectedPrice.Value != profile.PricePerMonth)
        {
            throw new InvalidOperationException(
                "The trainer's price has changed since you last viewed their profile. Refresh trainer details and try again.");
        }

        if (await _orders.HasPendingOrderAsync(clientUserId, profile.Id, cancellationToken))
            throw new InvalidOperationException("You already have a pending booking with this trainer.");

        var trainerUserId = profile.UserId;
        var snapshotPrice = profile.PricePerMonth;

        var order = new TrainerOrder
        {
            Id = Guid.NewGuid(),
            UserId = clientUserId,
            TrainerProfileId = profile.Id,
            TrainerId = trainerUserId,
            Price = snapshotPrice,
            Status = TrainerOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _orders.GetByIdWithPartiesAsync(order.Id, cancellationToken);
        if (created is null)
            throw new InvalidOperationException("Booking was created but could not be loaded.");

        return _mapper.Map<TrainerOrderDto>(created);
    }

    public async Task<PagedResult<TrainerOrderDto>> ListMyBookingsAsync(
        Guid clientUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var (orders, total) = await _orders.ListByClientPagedAsync(clientUserId, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<TrainerOrderDto>>(orders);
        return PagedResult<TrainerOrderDto>.Create(items, total, page, pageSize);
    }

    public async Task<PagedResult<TrainerOrderDto>> ListTrainerOrdersAsync(
        Guid trainerUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await _trainerProfiles.GetByUserIdWithUserAsync(trainerUserId, cancellationToken);
        if (profile is null)
            throw new KeyNotFoundException("Trainer profile was not found for this user.");

        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var (orders, total) = await _orders.ListByTrainerProfileIdPagedAsync(profile.Id, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<TrainerOrderDto>>(orders);
        return PagedResult<TrainerOrderDto>.Create(items, total, page, pageSize);
    }

    private static TrainerSummaryDto MapSummary(TrainerProfile profile)
    {
        return new TrainerSummaryDto
        {
            TrainerProfileId = profile.Id,
            TrainerUserId = profile.UserId,
            FullName = profile.User.FullName,
            Bio = profile.Bio,
            ExperienceYears = profile.ExperienceYears,
            PricePerMonth = profile.PricePerMonth,
            Rating = profile.Rating
        };
    }

    private static TrainerDetailDto MapDetail(TrainerProfile profile)
    {
        return new TrainerDetailDto
        {
            TrainerProfileId = profile.Id,
            TrainerUserId = profile.UserId,
            FullName = profile.User.FullName,
            Email = profile.User.Email,
            Bio = profile.Bio,
            ExperienceYears = profile.ExperienceYears,
            PricePerMonth = profile.PricePerMonth,
            Rating = profile.Rating,
            IsApproved = profile.IsApproved
        };
    }
}
