using GymFit.Domain.Enums;

namespace GymFit.Api.Authorization;

/// <summary>JWT and authorization policy role names (must match <see cref="UserRole"/> enum names).</summary>
public static class AppRoles
{
    public const string User = nameof(UserRole.User);
    public const string Trainer = nameof(UserRole.Trainer);
    public const string Admin = nameof(UserRole.Admin);
}
