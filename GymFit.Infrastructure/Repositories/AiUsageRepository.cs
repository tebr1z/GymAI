using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class AiUsageRepository : Repository<AiUsageLedger>, IAiUsageRepository
{
    public AiUsageRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<int> GetRequestCountAsync(
        Guid userId,
        string periodKey,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(x => x.UserId == userId && x.PeriodKey == periodKey)
            .Select(x => x.RequestCount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AiUsageLedger?> GetTrackedLedgerAsync(
        Guid userId,
        string periodKey,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsTracking().FirstOrDefaultAsync(
            x => x.UserId == userId && x.PeriodKey == periodKey,
            cancellationToken);
    }
}
