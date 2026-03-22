using GymFit.Api.Extensions;
using Microsoft.AspNetCore.Http;

namespace GymFit.Api.Middlewares;

/// <summary>
/// Runs before the built-in JWT bearer handler. Rejects syntactically invalid Bearer tokens early
/// so downstream middleware does not process garbage. Actual signature and lifetime validation
/// still happen in <see cref="Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler"/> via <c>UseAuthentication()</c>.
/// </summary>
public sealed class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var headerValues))
        {
            await _next(context);
            return;
        }

        var header = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(header))
        {
            await _next(context);
            return;
        }

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var token = header["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            await WriteUnauthorizedAsync(context, "Bearer token is missing.");
            return;
        }

        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            _logger.LogWarning("Rejected malformed JWT (expected 3 segments) for {Path}", context.Request.Path);
            await WriteUnauthorizedAsync(context, "Malformed JWT token.");
            return;
        }

        await _next(context);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context, string detail) =>
        context.WriteGlobalErrorAsync(
            StatusCodes.Status401Unauthorized,
            "The access token is missing or not valid for this request.",
            detail);
}
