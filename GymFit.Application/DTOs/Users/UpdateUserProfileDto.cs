namespace GymFit.Application.DTOs.Users;

public sealed class UpdateUserProfileDto
{
    public string? FullName { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? Goal { get; set; }
}
