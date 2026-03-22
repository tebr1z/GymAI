namespace GymFit.Application.DTOs.Trainers;

public sealed class TrainerSummaryDto
{
    public Guid TrainerProfileId { get; set; }
    public Guid TrainerUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public decimal PricePerMonth { get; set; }
    public decimal Rating { get; set; }
}
