using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Infrastructure.Repositories;

public sealed class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Message> Items, bool HasMore)> GetConversationPageAsync(
        Guid userAId,
        Guid userBId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var fetch = pageSize + 1;
        var skip = (page - 1) * pageSize;

        var batch = await DbSet.AsNoTracking()
            .Where(m =>
                (m.SenderId == userAId && m.ReceiverId == userBId) ||
                (m.SenderId == userBId && m.ReceiverId == userAId))
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Skip(skip)
            .Take(fetch)
            .ToListAsync(cancellationToken);

        var hasMore = batch.Count > pageSize;
        var pageItems = batch.Take(pageSize).ToList();
        pageItems.Reverse();
        return (pageItems, hasMore);
    }

    public async Task<(IReadOnlyList<Message> Items, bool HasMore)> GetInboxPageAsync(
        Guid receiverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var fetch = pageSize + 1;
        var skip = (page - 1) * pageSize;

        var batch = await DbSet.AsNoTracking()
            .Where(m => m.ReceiverId == receiverId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Skip(skip)
            .Take(fetch)
            .ToListAsync(cancellationToken);

        var hasMore = batch.Count > pageSize;
        var pageItems = batch.Take(pageSize).ToList();
        pageItems.Reverse();
        return (pageItems, hasMore);
    }
}
