namespace GymFit.Application.DTOs.Messages;

public sealed class MessagePaginationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
