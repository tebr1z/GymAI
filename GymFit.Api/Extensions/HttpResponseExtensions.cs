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

        var payload = new GlobalErrorResponse
        {
            Success = false,
            Message = message,
            Details = details
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.Options));
    }
}
