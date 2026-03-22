using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymFit.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var id))
            throw new UnauthorizedAccessException("The access token does not contain a valid user id.");
        return id;
    }
}
