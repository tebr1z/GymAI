using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Subscription?> GetActiveForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(s =>
                s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && s.StartDate <= utcNow
                && s.EndDate >= utcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> ListActiveTrackedForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsTracking()
            .Where(s =>
                s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= utcNow)
            .ToListAsync(cancellationToken);
    }
}
