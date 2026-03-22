namespace GymFit.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string RequireUser = nameof(RequireUser);
    public const string RequireTrainer = nameof(RequireTrainer);
    public const string RequireAdmin = nameof(RequireAdmin);
}
