using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GymFit.Application.Common;
using GymFit.Application.Configuration;
using GymFit.Application.DTOs.AI;
using GymFit.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymFit.Infrastructure.Services;

public sealed class OpenAiService : IAIService
{
    private const string HttpClientName = "OpenAi";

    /// <summary>Shown to clients when the OpenAI call cannot be completed successfully.</summary>
    public const string AiUnavailableUserMessage = "AI is temporarily unavailable, please try again later";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAiSettings _settings;
    private readonly IAiQuotaService _quota;
    private readonly ILogger<OpenAiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAiSettings> settings,
        IAiQuotaService quota,
        ILogger<OpenAiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _quota = quota;
        _logger = logger;
    }

    public Task<ServiceResult<AiPlanResponseDto>> GenerateWorkoutPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(
            userId,
            userInput,
            "You are a certified fitness coach. Produce a clear, safe workout plan tailored to the user's goals and constraints. Use headings and bullet points. If information is missing, state reasonable assumptions.",
            cancellationToken);

    public Task<ServiceResult<AiPlanResponseDto>> GenerateDietPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(
            userId,
            userInput,
            "You are a registered dietitian assistant. Produce practical meal guidance and macro-oriented suggestions. Avoid medical diagnoses; encourage consulting a professional for conditions. Use headings and bullet points.",
            cancellationToken);

    private async Task<ServiceResult<AiPlanResponseDto>> GenerateAsync(
        Guid userId,
        string userInput,
        string systemPrompt,
        CancellationToken cancellationToken)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return ServiceResult<AiPlanResponseDto>.Fail(
                    "Invalid user id.",
                    ServiceFailureKind.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogWarning("OpenAI request skipped: API key is not configured.");
                return AiUnavailable();
            }

            var quotaCheck = await _quota.EnsureWithinMonthlyAiQuotaAsync(userId, cancellationToken).ConfigureAwait(false);
            if (!quotaCheck.IsSuccess)
            {
                return ServiceResult<AiPlanResponseDto>.Fail(
                    quotaCheck.Error ?? "AI quota check failed.",
                    quotaCheck.Kind);
            }

            var client = _httpClientFactory.CreateClient(HttpClientName);
            var requestBody = new ChatCompletionRequest(
                _settings.Model,
                new[]
                {
                    new ChatMessage("system", systemPrompt),
                    new ChatMessage("user", userInput)
                });

            var maxAttempts = Math.Max(1, 1 + Math.Max(0, _settings.MaxRetries));
            string? lastModel = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var response = await client
                        .PostAsJsonAsync("chat/completions", requestBody, JsonOptions, cancellationToken)
                        .ConfigureAwait(false);

                    var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (raw.Length > Math.Max(1024, _settings.MaxRawResponseCharacters))
                    {
                        _logger.LogWarning(
                            "OpenAI response too large ({Length} chars) for user {UserId}, attempt {Attempt}",
                            raw.Length,
                            userId,
                            attempt);
                        if (ShouldRetryAfterValidationFailure(attempt, maxAttempts))
                        {
                            await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        return AiUnavailable();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var validated = TryValidateCompletionPayload(raw, userId, out var content, out lastModel);
                        if (validated)
                        {
                            var record = await _quota.RecordSuccessfulAiCallAsync(userId, cancellationToken)
                                .ConfigureAwait(false);
                            if (!record.IsSuccess)
                            {
                                _logger.LogError(
                                    "AI completion validated but usage could not be recorded for user {UserId}: {Error}",
                                    userId,
                                    record.Error);
                                return AiUnavailable();
                            }

                            return ServiceResult<AiPlanResponseDto>.Ok(new AiPlanResponseDto
                            {
                                Content = content!,
                                Model = lastModel ?? _settings.Model
                            });
                        }

                        _logger.LogWarning(
                            "OpenAI returned 200 but payload failed validation for user {UserId}, attempt {Attempt}",
                            userId,
                            attempt);
                        if (ShouldRetryAfterValidationFailure(attempt, maxAttempts))
                        {
                            await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        return AiUnavailable();
                    }

                    var transient = IsTransientHttpFailure(response.StatusCode);
                    _logger.LogWarning(
                        "OpenAI HTTP {StatusCode} for user {UserId}, attempt {Attempt}/{MaxAttempts}. Body length: {Length}",
                        (int)response.StatusCode,
                        userId,
                        attempt,
                        maxAttempts,
                        raw.Length);

                    if (transient && attempt < maxAttempts)
                    {
                        await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return AiUnavailable();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "OpenAI request timed out or was cancelled by the HTTP stack for user {UserId}, attempt {Attempt}/{MaxAttempts}",
                        userId,
                        attempt,
                        maxAttempts);
                    if (attempt < maxAttempts)
                    {
                        await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return AiUnavailable();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "OpenAI HTTP transport error for user {UserId}, attempt {Attempt}/{MaxAttempts}",
                        userId,
                        attempt,
                        maxAttempts);
                    if (attempt < maxAttempts)
                    {
                        await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return AiUnavailable();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error calling OpenAI for user {UserId}, attempt {Attempt}", userId, attempt);
                    if (attempt < maxAttempts)
                    {
                        await BackoffDelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return AiUnavailable();
                }
            }

            return AiUnavailable();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled path in OpenAI pipeline for user {UserId}", userId);
            return AiUnavailable();
        }
    }

    private bool TryValidateCompletionPayload(
        string raw,
        Guid userId,
        out string? content,
        out string? model)
    {
        content = null;
        model = null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out var errorEl))
            {
                _logger.LogWarning(
                    "OpenAI returned error object for user {UserId}: {Error}",
                    userId,
                    errorEl.GetRawText());
                return false;
            }

            ChatCompletionResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(raw, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize OpenAI completion JSON for user {UserId}", userId);
                return false;
            }

            model = parsed?.Model;
            var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                _logger.LogWarning("OpenAI completion missing assistant content for user {UserId}", userId);
                return false;
            }

            var minLen = Math.Max(1, _settings.MinAssistantContentLength);
            var maxLen = Math.Max(minLen, _settings.MaxAssistantContentLength);
            if (text.Length < minLen)
            {
                _logger.LogWarning(
                    "OpenAI assistant content too short ({Length}) for user {UserId}",
                    text.Length,
                    userId);
                return false;
            }

            if (text.Length > maxLen)
            {
                _logger.LogWarning(
                    "OpenAI assistant content exceeds max length ({Length}) for user {UserId}",
                    text.Length,
                    userId);
                return false;
            }

            content = text;
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "OpenAI raw JSON could not be parsed for user {UserId}", userId);
            return false;
        }
    }

    private static bool ShouldRetryAfterValidationFailure(int attempt, int maxAttempts) => attempt < maxAttempts;

    private async Task BackoffDelayAsync(int attemptIndex, CancellationToken cancellationToken)
    {
        var baseMs = Math.Max(50, _settings.RetryBaseDelayMilliseconds);
        var delay = TimeSpan.FromMilliseconds(baseMs * Math.Pow(2, attemptIndex - 1));
        var capped = delay > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delay;
        try
        {
            await Task.Delay(capped, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    private static bool IsTransientHttpFailure(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private ServiceResult<AiPlanResponseDto> AiUnavailable() =>
        ServiceResult<AiPlanResponseDto>.Fail(AiUnavailableUserMessage, ServiceFailureKind.ServiceUnavailable);

    private sealed record ChatCompletionRequest(string Model, ChatMessage[] Messages);

    private sealed record ChatMessage(string Role, string Content);

    private sealed record ChatCompletionResponse(ChatChoice[]? Choices, string? Model);

    private sealed record ChatChoice(ChatMessageResponse? Message);

    private sealed record ChatMessageResponse(string? Content);
}
