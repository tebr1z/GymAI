using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<(IReadOnlyList<Plan> Items, int TotalCount)> ListByUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Plan?> GetByIdWithUsersAsync(Guid planId, CancellationToken cancellationToken = default);
}
