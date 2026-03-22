namespace GymFit.Api.Models;

/// <summary>Marker for <see cref="ApiEnvelopeResultFilter"/> to avoid double-wrapping.</summary>
public interface IApiEnvelope;

public abstract class ApiEnvelopeBase : IApiEnvelope
{
    public bool Success { get; init; } = true;
    public string? TraceId { get; init; }
}

/// <summary>Standard success payload for JSON APIs (applied by <see cref="Filters.ApiEnvelopeResultFilter"/>).</summary>
public sealed class ApiEnvelope<T> : ApiEnvelopeBase
{
    public T? Data { get; init; }

    public static ApiEnvelope<T> Ok(T? data, string? traceId) =>
        new() { Data = data, TraceId = traceId, Success = true };
}
