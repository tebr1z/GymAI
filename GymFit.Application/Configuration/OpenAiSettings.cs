namespace GymFit.Application.Configuration;

public sealed class OpenAiSettings
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Per-request timeout for the HTTP client (each attempt).</summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>Extra retries after the first attempt for transient failures (429, 5xx, timeouts, network).</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Base delay for exponential backoff (ms): base * 2^attemptIndex.</summary>
    public int RetryBaseDelayMilliseconds { get; set; } = 750;

    public int MinAssistantContentLength { get; set; } = 8;
    public int MaxAssistantContentLength { get; set; } = 200_000;

    /// <summary>Reject responses larger than this to avoid excessive memory use.</summary>
    public int MaxRawResponseCharacters { get; set; } = 2_000_000;
}
