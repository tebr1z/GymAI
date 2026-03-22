using GymFit.Application.DTOs.Users;

namespace GymFit.Application.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string TokenType { get; set; } = "Bearer";
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = null!;
}
