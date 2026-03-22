using GymFit.Domain.Enums;

namespace GymFit.Application.DTOs.Users;

public sealed class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? Goal { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasTrainerProfile { get; set; }
}
