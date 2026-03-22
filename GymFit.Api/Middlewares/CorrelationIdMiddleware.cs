using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace GymFit.Api.Middlewares;

/// <summary>Propagates or generates a correlation id for logs, responses, and error bodies.</summary>
public sealed class CorrelationIdMiddleware
{
    public const string CorrelationIdItemKey = "GymFit.CorrelationId";
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Items[CorrelationIdItemKey] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues header)
            && !StringValues.IsNullOrEmpty(header)
            && !string.IsNullOrWhiteSpace(header.ToString()))
            return header.ToString().Trim();

        return Guid.NewGuid().ToString("N");
    }
}
