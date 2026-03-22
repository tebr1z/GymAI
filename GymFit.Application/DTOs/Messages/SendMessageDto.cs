namespace GymFit.Application.DTOs.Messages;

public sealed class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    public string MessageText { get; set; } = string.Empty;
}
