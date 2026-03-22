using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface IAiUsageRepository : IRepository<AiUsageLedger>
{
    Task<int> GetRequestCountAsync(Guid userId, string periodKey, CancellationToken cancellationToken = default);

    Task<AiUsageLedger?> GetTrackedLedgerAsync(Guid userId, string periodKey, CancellationToken cancellationToken = default);
}
