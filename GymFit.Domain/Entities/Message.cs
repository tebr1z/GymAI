namespace GymFit.Domain.Entities;

public class Message : BaseEntity
{
    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;

    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
