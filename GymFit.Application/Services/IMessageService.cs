using GymFit.Application.DTOs.Messages;

namespace GymFit.Application.Services;

public interface IMessageService
{
    Task<MessageDto> SendAsync(Guid senderId, SendMessageDto request, CancellationToken cancellationToken = default);

    Task<MessageListPageDto> GetConversationPageAsync(
        Guid userId,
        Guid peerUserId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<MessageListPageDto> GetInboxPageAsync(
        Guid userId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default);
}
