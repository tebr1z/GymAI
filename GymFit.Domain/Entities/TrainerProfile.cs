namespace GymFit.Domain.Entities;

public class TrainerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public decimal PricePerMonth { get; set; }
    public decimal Rating { get; set; }
    public bool IsApproved { get; set; }

    public ICollection<TrainerOrder> TrainerOrders { get; set; } = new List<TrainerOrder>();
}
