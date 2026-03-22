using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface ITrainerOrderRepository : IRepository<TrainerOrder>
{
    Task<TrainerOrder?> GetByIdWithPartiesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TrainerOrder> Items, int TotalCount)> ListByClientPagedAsync(
        Guid clientUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TrainerOrder> Items, int TotalCount)> ListByTrainerProfileIdPagedAsync(
        Guid trainerProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingOrderAsync(
        Guid clientUserId,
        Guid trainerProfileId,
        CancellationToken cancellationToken = default);
}
