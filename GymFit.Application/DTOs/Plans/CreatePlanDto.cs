using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Plans;

public sealed class CreatePlanDto
{
    public FitnessPlanType Type { get; set; }
    public string Content { get; set; } = "{}";
    public Guid? TrainerId { get; set; }
}
