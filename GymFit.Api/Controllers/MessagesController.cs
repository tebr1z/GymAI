using GymFit.Api.Authorization;
using GymFit.Api.Extensions;
using GymFit.Application.DTOs.Messages;
using GymFit.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

/// <summary>Direct messaging between members and trainers.</summary>
[Route("api/messages")]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
public sealed class MessagesController : ApiV1ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>Send a message to another user (member ↔ trainer).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageDto>> Send(
        [FromBody] SendMessageDto request,
        CancellationToken cancellationToken)
    {
        var senderId = User.GetRequiredUserId();
        var result = await _messageService.SendAsync(senderId, request, cancellationToken);
        return MapToActionResult(result);
    }

    /// <summary>
    /// Paginated thread with <paramref name="peerUserId"/>. Page 1 is the newest slice; items are oldest→newest within the page.
    /// </summary>
    [HttpGet("conversations/{peerUserId:guid}")]
    [ProducesResponseType(typeof(MessageListPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageListPageDto>> GetConversation(
        Guid peerUserId,
        [FromQuery] MessagePaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await _messageService.GetConversationPageAsync(userId, peerUserId, pagination, cancellationToken);
        return MapToActionResult(result);
    }

    /// <summary>Received messages, paginated (newest page first; items oldest→newest within the page).</summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(MessageListPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageListPageDto>> Inbox(
        [FromQuery] MessagePaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var result = await _messageService.GetInboxPageAsync(userId, pagination, cancellationToken);
        return MapToActionResult(result);
    }
}
