using GymFit.Domain.Entities;

namespace GymFit.Application.Repositories;

public interface IMessageRepository : IRepository<Message>
{
    /// <summary>
    /// One-based page of messages between two users, newest page first.
    /// Uses (pageSize + 1) fetch to compute <paramref name="hasMore"/> without a separate count query.
    /// </summary>
    Task<(IReadOnlyList<Message> Items, bool HasMore)> GetConversationPageAsync(
        Guid userAId,
        Guid userBId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Message> Items, bool HasMore)> GetInboxPageAsync(
        Guid receiverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
