namespace GymFit.Api.Models;

/// <summary>Consistent JSON shape for API errors (middleware, validation, JWT pre-check).</summary>
public sealed class GlobalErrorResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    /// <summary>Optional technical or field-level detail (omit or null when not needed).</summary>
    public string? Details { get; init; }
}
