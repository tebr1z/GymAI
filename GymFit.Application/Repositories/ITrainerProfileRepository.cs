using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface ITrainerProfileRepository : IRepository<TrainerProfile>
{
    Task<TrainerProfile?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TrainerProfile?> GetByUserIdWithUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TrainerProfile> Items, int TotalCount)> ListApprovedMarketplaceAsync(
        int page,
        int pageSize,
        decimal? minRating,
        decimal? maxPricePerMonth,
        CancellationToken cancellationToken = default);
}
