using GymFit.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace GymFit.Api.Authorization;

/// <summary>
/// Returns the standard <see cref="Models.ApiResponse{T}"/> JSON shape for 401/403 instead of empty or non-JSON bodies.
/// </summary>
public sealed class StandardApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (context.Response.HasStarted)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await context.WriteGlobalErrorAsync(
                StatusCodes.Status403Forbidden,
                "You are not allowed to access this resource.").ConfigureAwait(false);
            return;
        }

        await context.WriteGlobalErrorAsync(
            StatusCodes.Status401Unauthorized,
            "Authentication required.").ConfigureAwait(false);
    }
}
