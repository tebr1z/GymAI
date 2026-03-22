using GymFit.Api.Middlewares;

namespace GymFit.Api.Extensions;

public static class HttpContextExtensions
{
    public static string GetCorrelationId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var value)
            && value is string id
            && !string.IsNullOrWhiteSpace(id))
            return id;

        return httpContext.TraceIdentifier;
    }
}
