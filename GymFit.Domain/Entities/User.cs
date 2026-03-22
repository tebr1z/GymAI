using GymFit.Domain.Enums;

namespace GymFit.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? Goal { get; set; }
    public DateTime CreatedAt { get; set; }

    public TrainerProfile? TrainerProfile { get; set; }
    public ICollection<Plan> Plans { get; set; } = new List<Plan>();
    public ICollection<Plan> CoachingPlans { get; set; } = new List<Plan>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<AiUsageLedger> AiUsageLedgers { get; set; } = new List<AiUsageLedger>();
    public ICollection<TrainerOrder> ClientOrders { get; set; } = new List<TrainerOrder>();
    public ICollection<TrainerOrder> TrainerOrders { get; set; } = new List<TrainerOrder>();
}
