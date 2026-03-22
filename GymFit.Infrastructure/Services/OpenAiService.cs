using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GymFit.Application.Configuration;
using GymFit.Application.DTOs.AI;
using GymFit.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymFit.Infrastructure.Services;

public sealed class OpenAiService : IAIService
{
    private const string HttpClientName = "OpenAi";

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

    public Task<AiPlanResponseDto> GenerateWorkoutPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(
            userId,
            userInput,
            "You are a certified fitness coach. Produce a clear, safe workout plan tailored to the user's goals and constraints. Use headings and bullet points. If information is missing, state reasonable assumptions.",
            cancellationToken);

    public Task<AiPlanResponseDto> GenerateDietPlanAsync(
        Guid userId,
        string userInput,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(
            userId,
            userInput,
            "You are a registered dietitian assistant. Produce practical meal guidance and macro-oriented suggestions. Avoid medical diagnoses; encourage consulting a professional for conditions. Use headings and bullet points.",
            cancellationToken);

    private async Task<AiPlanResponseDto> GenerateAsync(
        Guid userId,
        string userInput,
        string systemPrompt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("OpenAI API key is not configured.");

        await _quota.EnsureWithinMonthlyAiQuotaAsync(userId, cancellationToken);

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var requestBody = new ChatCompletionRequest(
                _settings.Model,
                new[]
                {
                    new ChatMessage("system", systemPrompt),
                    new ChatMessage("user", userInput)
                });

            using var response = await client.PostAsJsonAsync("chat/completions", requestBody, JsonOptions, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenAI API error {StatusCode} for user {UserId}. BodyLength: {BodyLength}",
                    (int)response.StatusCode,
                    userId,
                    raw.Length);
                throw new InvalidOperationException("The AI service returned an error. Please try again later.");
            }

            ChatCompletionResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(raw, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI JSON response for user {UserId}", userId);
                throw new InvalidOperationException("The AI service returned an unexpected response.");
            }

            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("The AI service returned empty content.");

            await _quota.RecordSuccessfulAiCallAsync(userId, cancellationToken);

            return new AiPlanResponseDto
            {
                Content = content,
                Model = parsed?.Model ?? _settings.Model
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OpenAI HTTP request failed for user {UserId}", userId);
            throw new InvalidOperationException(
                "The AI service is temporarily unreachable. Please try again in a few moments.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling OpenAI for user {UserId}", userId);
            throw new InvalidOperationException(
                "The AI request could not be completed. Please try again later.");
        }
    }

    private sealed record ChatCompletionRequest(string Model, ChatMessage[] Messages);

    private sealed record ChatMessage(string Role, string Content);

    private sealed record ChatCompletionResponse(ChatChoice[]? Choices, string? Model);

    private sealed record ChatChoice(ChatMessageResponse? Message);

    private sealed record ChatMessageResponse(string? Content);
}
