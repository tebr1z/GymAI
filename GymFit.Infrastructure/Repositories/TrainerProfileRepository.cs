using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class TrainerProfileRepository : Repository<TrainerProfile>, ITrainerProfileRepository
{
    public TrainerProfileRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<TrainerProfile?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TrainerProfile?> GetByUserIdWithUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainerProfile>> ListApprovedAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.IsApproved)
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.User.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<TrainerProfile> Items, int TotalCount)> ListApprovedMarketplaceAsync(
        int page,
        int pageSize,
        decimal? minRating,
        decimal? maxPricePerMonth,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TrainerProfile> query = DbSet.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.IsApproved);

        if (minRating.HasValue)
            query = query.Where(x => x.Rating >= minRating.Value);

        if (maxPricePerMonth.HasValue)
            query = query.Where(x => x.PricePerMonth <= maxPricePerMonth.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.User.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
