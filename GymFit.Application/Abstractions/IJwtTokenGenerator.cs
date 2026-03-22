using GymFit.Domain.Enums;

namespace GymFit.Application.Abstractions;

public interface IJwtTokenGenerator
{
    AccessTokenResult CreateAccessToken(Guid userId, string email, string fullName, UserRole role);
}

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
