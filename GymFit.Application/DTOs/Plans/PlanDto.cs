using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Plans;

public sealed class PlanDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TrainerId { get; set; }
    public FitnessPlanType Type { get; set; }
    public string Content { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
