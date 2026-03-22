using AutoMapper;
using GymFit.Application.Abstractions;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Messages;
using GymFit.Application.Repositories;
using GymFit.Domain.Entities;
using GymFit.Domain.Enums;

namespace GymFit.Application.Services;

public sealed class MessageService : IMessageService
{
    private readonly IMessageRepository _messages;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageService(
        IMessageRepository messages,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _messages = messages;
        _users = users;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MessageDto> SendAsync(Guid senderId, SendMessageDto request, CancellationToken cancellationToken = default)
    {
        if (senderId == request.ReceiverId)
            throw new InvalidOperationException("You cannot send a message to yourself.");

        var sender = await _users.GetByIdAsync(senderId, cancellationToken);
        if (sender is null)
            throw new KeyNotFoundException("Sender was not found.");

        var receiver = await _users.GetByIdAsync(request.ReceiverId, cancellationToken);
        if (receiver is null)
            throw new KeyNotFoundException("Receiver was not found.");

        if (!IsTrainerParticipant(sender.Role) && !IsTrainerParticipant(receiver.Role))
        {
            throw new InvalidOperationException(
                "Messaging is only supported between members and trainers. At least one participant must be a trainer.");
        }

        var text = request.MessageText.Trim();
        if (string.IsNullOrEmpty(text))
            throw new InvalidOperationException("Message text cannot be empty.");

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

        var created = await _messages.GetByIdAsync(message.Id, cancellationToken)
                      ?? throw new InvalidOperationException("Message was sent but could not be loaded.");

        return _mapper.Map<MessageDto>(created);
    }

    public async Task<MessageListPageDto> GetConversationPageAsync(
        Guid userId,
        Guid peerUserId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        if (userId == peerUserId)
            throw new InvalidOperationException("Invalid conversation.");

        if (!await _users.ExistsAsync(userId, cancellationToken))
            throw new KeyNotFoundException("User was not found.");

        if (!await _users.ExistsAsync(peerUserId, cancellationToken))
            throw new KeyNotFoundException("The other participant was not found.");

        var (page, pageSize) = Pagination.Normalize(pagination.Page, pagination.PageSize);

        var (items, hasMore) = await _messages.GetConversationPageAsync(
            userId,
            peerUserId,
            page,
            pageSize,
            cancellationToken);

        return new MessageListPageDto
        {
            Items = _mapper.Map<IReadOnlyList<MessageDto>>(items),
            Page = page,
            PageSize = pageSize,
            HasMore = hasMore
        };
    }

    public async Task<MessageListPageDto> GetInboxPageAsync(
        Guid userId,
        MessagePaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        if (!await _users.ExistsAsync(userId, cancellationToken))
            throw new KeyNotFoundException("User was not found.");

        var (page, pageSize) = Pagination.Normalize(pagination.Page, pagination.PageSize);

        var (items, hasMore) = await _messages.GetInboxPageAsync(userId, page, pageSize, cancellationToken);

        return new MessageListPageDto
        {
            Items = _mapper.Map<IReadOnlyList<MessageDto>>(items),
            Page = page,
            PageSize = pageSize,
            HasMore = hasMore
        };
    }

    private static bool IsTrainerParticipant(UserRole role) =>
        role is UserRole.Trainer or UserRole.Admin;
}
