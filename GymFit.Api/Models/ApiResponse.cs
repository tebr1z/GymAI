namespace GymFit.Api.Models;

/// <summary>Marker for result filters: body is already the standard API shape.</summary>
public interface IApiStandardEnvelope;

/// <summary>
/// Standard JSON envelope for every API response: <c>success</c>, <c>message</c>, <c>data</c>, <c>errors</c>.
/// </summary>
public sealed class ApiResponse<T> : IApiStandardEnvelope
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    /// <summary>Always an array in JSON (empty when there are no field-level or extra messages).</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ApiResponse<T> Ok(T? data, string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>()
        };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string> errors) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors.Count > 0 ? errors : Array.Empty<string>()
        };
}
