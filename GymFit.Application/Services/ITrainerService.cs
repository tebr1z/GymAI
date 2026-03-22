using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Services;

public interface ITrainerService
{
    Task<ServiceResult<PagedResult<TrainerSummaryDto>>> ListMarketplaceAsync(
        TrainerMarketplaceQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TrainerDetailDto>> GetTrainerAsync(Guid trainerProfileId, CancellationToken cancellationToken = default);

    Task<ServiceResult<TrainerOrderDto>> BookTrainerAsync(
        Guid clientUserId,
        BookTrainerDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<TrainerOrderDto>>> ListMyBookingsAsync(
        Guid clientUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<TrainerOrderDto>>> ListTrainerOrdersAsync(
        Guid trainerUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);
}
