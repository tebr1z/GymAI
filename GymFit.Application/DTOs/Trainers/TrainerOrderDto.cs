using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Trainers;

public sealed class TrainerOrderDto
{
    public Guid Id { get; set; }
    public Guid ClientUserId { get; set; }
    public string? ClientFullName { get; set; }
    public Guid TrainerProfileId { get; set; }
    public Guid TrainerUserId { get; set; }
    public string? TrainerFullName { get; set; }
    public decimal Price { get; set; }
    public TrainerOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
