using GymFit.Domain.Enums;

namespace GymFit.Domain.Entities;

public class TrainerOrder : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TrainerProfileId { get; set; }
    public TrainerProfile TrainerProfile { get; set; } = null!;

    /// <summary>Trainer user id; must match <see cref="TrainerProfile"/>.UserId.</summary>
    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    public decimal Price { get; set; }
    public TrainerOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
