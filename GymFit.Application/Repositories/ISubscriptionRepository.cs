using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> ListActiveTrackedForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
