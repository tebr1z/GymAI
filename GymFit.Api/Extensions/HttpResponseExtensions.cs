using System.Text.Json;
using GymFit.Api.Infrastructure;
using GymFit.Api.Models;

namespace GymFit.Api.Extensions;

public static class HttpResponseExtensions
{
    public static async Task WriteGlobalErrorAsync(
        this HttpContext httpContext,
        int statusCode,
        string message,
        string? details = null)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        var errors = string.IsNullOrWhiteSpace(details)
            ? Array.Empty<string>()
            : new[] { details };

        var payload = ApiResponse<object?>.Fail(message, errors);

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.Options));
    }
}
