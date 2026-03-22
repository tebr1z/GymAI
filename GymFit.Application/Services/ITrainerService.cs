using GymFit.Application.DTOs.Common;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Services;

public interface ITrainerService
{
    Task<PagedResult<TrainerSummaryDto>> ListMarketplaceAsync(
        TrainerMarketplaceQuery query,
        CancellationToken cancellationToken = default);

    Task<TrainerDetailDto> GetTrainerAsync(Guid trainerProfileId, CancellationToken cancellationToken = default);

    Task<TrainerOrderDto> BookTrainerAsync(
        Guid clientUserId,
        BookTrainerDto request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TrainerOrderDto>> ListMyBookingsAsync(
        Guid clientUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TrainerOrderDto>> ListTrainerOrdersAsync(
        Guid trainerUserId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);
}
