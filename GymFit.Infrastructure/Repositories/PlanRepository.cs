using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Plan> Items, int TotalCount)> ListByUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Plan?> GetByIdWithUsersAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Trainer)
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken);
    }
}
