using GymFit.Domain.Enums;

namespace GymFit.Domain.Entities;

public class Plan : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? TrainerId { get; set; }
    public User? Trainer { get; set; }

    public FitnessPlanType Type { get; set; }
    public string Content { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
