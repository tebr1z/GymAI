using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Trainers;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Services;

public sealed class TrainerService : ITrainerService
{
    private readonly ITrainerProfileRepository _trainerProfiles;
    private readonly ITrainerOrderRepository _orders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TrainerService> _logger;

    public TrainerService(
        ITrainerProfileRepository trainerProfiles,
        ITrainerOrderRepository orders,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TrainerService> logger)
    {
        _trainerProfiles = trainerProfiles;
        _orders = orders;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ServiceResult<PagedResult<TrainerSummaryDto>>> ListMarketplaceAsync(
        TrainerMarketplaceQuery query,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(ListMarketplaceAsync), async () =>
        {
            var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);

            var (profiles, total) = await _trainerProfiles.ListApprovedMarketplaceAsync(
                page,
                pageSize,
                query.MinRating,
                query.MaxPricePerMonth,
                cancellationToken);

            var items = profiles.Select(MapSummary).ToList();

            return ServiceResult<PagedResult<TrainerSummaryDto>>.Ok(
                PagedResult<TrainerSummaryDto>.Create(items, total, page, pageSize));
        });

    public Task<ServiceResult<TrainerDetailDto>> GetTrainerAsync(
        Guid trainerProfileId,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetTrainerAsync), async () =>
        {
            if (trainerProfileId == Guid.Empty)
                return ServiceResult<TrainerDetailDto>.Fail("Invalid trainer profile id.", ServiceFailureKind.BadRequest);

            var profile = await _trainerProfiles.GetByIdWithUserAsync(trainerProfileId, cancellationToken);
            if (profile is null)
                return ServiceResult<TrainerDetailDto>.Fail("Trainer profile was not found.", ServiceFailureKind.NotFound);

            if (!profile.IsApproved)
            {
                return ServiceResult<TrainerDetailDto>.Fail(
                    "This trainer profile is not available in the marketplace.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult<TrainerDetailDto>.Ok(MapDetail(profile));
        });

    public Task<ServiceResult<TrainerOrderDto>> BookTrainerAsync(
        Guid clientUserId,
        BookTrainerDto request,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(BookTrainerAsync), async () =>
        {
            if (request is null)
                return ServiceResult<TrainerOrderDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            if (clientUserId == Guid.Empty)
                return ServiceResult<TrainerOrderDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (request.TrainerProfileId == Guid.Empty)
                return ServiceResult<TrainerOrderDto>.Fail("Invalid trainer profile id.", ServiceFailureKind.BadRequest);

            var profile = await _trainerProfiles.GetByIdWithUserAsync(request.TrainerProfileId, cancellationToken);
            if (profile is null)
                return ServiceResult<TrainerOrderDto>.Fail("Trainer profile was not found.", ServiceFailureKind.NotFound);

            if (!profile.IsApproved)
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "This trainer is not approved for bookings.",
                    ServiceFailureKind.BadRequest);
            }

            if (profile.UserId == clientUserId)
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "You cannot book yourself as a trainer.",
                    ServiceFailureKind.BadRequest);
            }

            if (profile.User.Role != UserRole.Trainer && profile.User.Role != UserRole.Admin)
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "The selected profile is not a trainer account.",
                    ServiceFailureKind.BadRequest);
            }

            if (profile.PricePerMonth < 0)
                return ServiceResult<TrainerOrderDto>.Fail("Trainer pricing is invalid.", ServiceFailureKind.BadRequest);

            if (request.ExpectedPrice.HasValue && request.ExpectedPrice.Value != profile.PricePerMonth)
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "The trainer's price has changed since you last viewed their profile. Refresh trainer details and try again.",
                    ServiceFailureKind.Conflict);
            }

            if (await _orders.HasPendingOrderAsync(clientUserId, profile.Id, cancellationToken))
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "You already have a pending booking with this trainer.",
                    ServiceFailureKind.Conflict);
            }

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
            {
                return ServiceResult<TrainerOrderDto>.Fail(
                    "Booking was created but could not be loaded.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult<TrainerOrderDto>.Ok(_mapper.Map<TrainerOrderDto>(created));
        });

    public Task<ServiceResult<PagedResult<TrainerOrderDto>>> ListMyBookingsAsync(
        Guid clientUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(ListMyBookingsAsync), async () =>
        {
            if (clientUserId == Guid.Empty)
                return ServiceResult<PagedResult<TrainerOrderDto>>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
            var (orders, total) = await _orders.ListByClientPagedAsync(clientUserId, page, pageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<TrainerOrderDto>>(orders);
            return ServiceResult<PagedResult<TrainerOrderDto>>.Ok(
                PagedResult<TrainerOrderDto>.Create(items, total, page, pageSize));
        });

    public Task<ServiceResult<PagedResult<TrainerOrderDto>>> ListTrainerOrdersAsync(
        Guid trainerUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(ListTrainerOrdersAsync), async () =>
        {
            if (trainerUserId == Guid.Empty)
                return ServiceResult<PagedResult<TrainerOrderDto>>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            var profile = await _trainerProfiles.GetByUserIdWithUserAsync(trainerUserId, cancellationToken);
            if (profile is null)
            {
                return ServiceResult<PagedResult<TrainerOrderDto>>.Fail(
                    "Trainer profile was not found for this user.",
                    ServiceFailureKind.NotFound);
            }

            var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
            var (orders, total) = await _orders.ListByTrainerProfileIdPagedAsync(profile.Id, page, pageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<TrainerOrderDto>>(orders);
            return ServiceResult<PagedResult<TrainerOrderDto>>.Ok(
                PagedResult<TrainerOrderDto>.Create(items, total, page, pageSize));
        });

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
