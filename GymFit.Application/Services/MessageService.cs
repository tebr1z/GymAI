using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Messages;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Services;

public sealed class MessageService : IMessageService
{
    private readonly IMessageRepository _messages;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IMessageRepository messages,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MessageService> logger)
    {
        _messages = messages;
        _users = users;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ServiceResult<MessageDto>> SendAsync(
        Guid senderId,
        SendMessageDto request,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(SendAsync), async () =>
        {
            if (request is null)
                return ServiceResult<MessageDto>.Fail("Request body is required.", ServiceFailureKind.BadRequest);

            if (senderId == Guid.Empty || request.ReceiverId == Guid.Empty)
                return ServiceResult<MessageDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (senderId == request.ReceiverId)
            {
                return ServiceResult<MessageDto>.Fail(
                    "You cannot send a message to yourself.",
                    ServiceFailureKind.BadRequest);
            }

            var sender = await _users.GetByIdAsync(senderId, cancellationToken);
            if (sender is null)
                return ServiceResult<MessageDto>.Fail("Sender was not found.", ServiceFailureKind.NotFound);

            var receiver = await _users.GetByIdAsync(request.ReceiverId, cancellationToken);
            if (receiver is null)
                return ServiceResult<MessageDto>.Fail("Receiver was not found.", ServiceFailureKind.NotFound);

            if (!IsTrainerParticipant(sender.Role) && !IsTrainerParticipant(receiver.Role))
            {
                return ServiceResult<MessageDto>.Fail(
                    "Messaging is only supported between members and trainers. At least one participant must be a trainer.",
                    ServiceFailureKind.BadRequest);
            }

            var text = request.MessageText.Trim();
            if (string.IsNullOrEmpty(text))
                return ServiceResult<MessageDto>.Fail("Message text cannot be empty.", ServiceFailureKind.BadRequest);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                MessageText = text,
                CreatedAt = DateTime.UtcNow
            };

            await _messages.AddAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await _messages.GetByIdAsync(message.Id, cancellationToken);
            if (created is null)
            {
                return ServiceResult<MessageDto>.Fail(
                    "Message was sent but could not be loaded.",
                    ServiceFailureKind.BadRequest);
            }

            return ServiceResult<MessageDto>.Ok(_mapper.Map<MessageDto>(created));
        });

    public Task<ServiceResult<MessageListPageDto>> GetConversationPageAsync(
        Guid userId,
        Guid peerUserId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetConversationPageAsync), async () =>
        {
            if (userId == Guid.Empty || peerUserId == Guid.Empty)
                return ServiceResult<MessageListPageDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (userId == peerUserId)
                return ServiceResult<MessageListPageDto>.Fail("Invalid conversation.", ServiceFailureKind.BadRequest);

            if (!await _users.ExistsAsync(userId, cancellationToken))
                return ServiceResult<MessageListPageDto>.Fail("User was not found.", ServiceFailureKind.NotFound);

            if (!await _users.ExistsAsync(peerUserId, cancellationToken))
            {
                return ServiceResult<MessageListPageDto>.Fail(
                    "The other participant was not found.",
                    ServiceFailureKind.NotFound);
            }

            var (page, pageSize) = Pagination.Normalize(pagination.Page, pagination.PageSize);

            var (items, hasMore) = await _messages.GetConversationPageAsync(
                userId,
                peerUserId,
                page,
                pageSize,
                cancellationToken);

            return ServiceResult<MessageListPageDto>.Ok(new MessageListPageDto
            {
                Items = _mapper.Map<IReadOnlyList<MessageDto>>(items),
                Page = page,
                PageSize = pageSize,
                HasMore = hasMore
            });
        });

    public Task<ServiceResult<MessageListPageDto>> GetInboxPageAsync(
        Guid userId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default) =>
        ServiceExecution.RunAsync(_logger, nameof(GetInboxPageAsync), async () =>
        {
            if (userId == Guid.Empty)
                return ServiceResult<MessageListPageDto>.Fail("Invalid user id.", ServiceFailureKind.BadRequest);

            if (!await _users.ExistsAsync(userId, cancellationToken))
                return ServiceResult<MessageListPageDto>.Fail("User was not found.", ServiceFailureKind.NotFound);

            var (page, pageSize) = Pagination.Normalize(pagination.Page, pagination.PageSize);

            var (items, hasMore) = await _messages.GetInboxPageAsync(userId, page, pageSize, cancellationToken);

            return ServiceResult<MessageListPageDto>.Ok(new MessageListPageDto
            {
                Items = _mapper.Map<IReadOnlyList<MessageDto>>(items),
                Page = page,
                PageSize = pageSize,
                HasMore = hasMore
            });
        });

    private static bool IsTrainerParticipant(UserRole role) =>
        role is UserRole.Trainer or UserRole.Admin;
}
