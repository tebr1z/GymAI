namespace GymFit.Application.DTOs.Messages;

/// <summary>
/// Paginated messages. Items are in chronological order (oldest first) within the page.
/// <see cref="HasMore"/> indicates another page exists (older for conversations, more inbox items).
/// </summary>
public sealed class MessageListPageDto
{
    public IReadOnlyList<MessageDto> Items { get; init; } = Array.Empty<MessageDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore { get; init; }
}
