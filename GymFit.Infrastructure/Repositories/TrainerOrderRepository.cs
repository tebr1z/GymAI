using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class TrainerOrderRepository : Repository<TrainerOrder>, ITrainerOrderRepository
{
    public TrainerOrderRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<TrainerOrder?> GetByIdWithPartiesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Trainer)
            .Include(x => x.TrainerProfile)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<TrainerOrder> Items, int TotalCount)> ListByClientPagedAsync(
        Guid clientUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Trainer)
            .Where(x => x.UserId == clientUserId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<TrainerOrder> Items, int TotalCount)> ListByTrainerProfileIdPagedAsync(
        Guid trainerProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Trainer)
            .Where(x => x.TrainerProfileId == trainerProfileId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> HasPendingOrderAsync(
        Guid clientUserId,
        Guid trainerProfileId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.UserId == clientUserId
                 && x.TrainerProfileId == trainerProfileId
                 && x.Status == TrainerOrderStatus.Pending,
            cancellationToken);
    }
}
