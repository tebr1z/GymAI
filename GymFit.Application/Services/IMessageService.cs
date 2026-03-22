using GymFit.Application.Common;
using GymFit.Application.DTOs.Messages;

namespace GymFit.Application.Services;

public interface IMessageService
{
    Task<ServiceResult<MessageDto>> SendAsync(Guid senderId, SendMessageDto request, CancellationToken cancellationToken = default);

    Task<ServiceResult<MessageListPageDto>> GetConversationPageAsync(
        Guid userId,
        Guid peerUserId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MessageListPageDto>> GetInboxPageAsync(
        Guid userId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default);
}
