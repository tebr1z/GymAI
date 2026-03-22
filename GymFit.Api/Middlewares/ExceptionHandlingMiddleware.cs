using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using GymFit.Api.Extensions;
using GymFit.Api.Infrastructure;
using GymFit.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Api.Middlewares;

/// <summary>
/// Catches unhandled exceptions, returns <see cref="ApiResponse{T}"/>, and logs full diagnostic context.
/// Does not catch exceptions after the response has started (those are logged and rethrown).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                LogException(context, ex, "Unhandled exception after response started; rethrowing.");
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        LogException(context, exception, "Unhandled exception while processing request.");

        var mapped = MapException(exception);

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = mapped.StatusCode;

        var details = mapped.Details;
        if (_environment.IsDevelopment() && mapped.StatusCode == StatusCodes.Status500InternalServerError)
            details = exception.ToString();

        var errors = FlattenExceptionErrors(exception, details);
        var payload = ApiResponse<object?>.Fail(mapped.Message, errors);

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.Options));
    }

    private static IReadOnlyList<string> FlattenExceptionErrors(Exception exception, string? details)
    {
        if (exception is ValidationException ve)
            return ve.Errors
                .Select(e => string.IsNullOrEmpty(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{ToCamelCaseKey(e.PropertyName)}: {e.ErrorMessage}")
                .ToList();

        if (!string.IsNullOrWhiteSpace(details))
            return new[] { details! };

        return Array.Empty<string>();
    }

    private static string ToCamelCaseKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key == "_")
            return key;
        if (!char.IsUpper(key[0]))
            return key;
        return char.ToLowerInvariant(key[0]) + key[1..];
    }

    private void LogException(HttpContext context, Exception exception, string headline)
    {
        var (userId, userEmail) = GetUserContext(context);

        _logger.LogError(
            exception,
            "{Headline} | Category: {ErrorCategory} | Path: {RequestPath} | Method: {HttpMethod} | " +
            "UserId: {UserId} | Email: {UserEmail} | IsAuthenticated: {IsAuthenticated} | " +
            "CorrelationId: {CorrelationId} | ExceptionType: {ExceptionType} | ExceptionMessage: {ExceptionMessage}",
            headline,
            GetErrorCategory(exception),
            context.Request.Path.Value,
            context.Request.Method,
            userId ?? "(anonymous)",
            userEmail ?? "(none)",
            context.User?.Identity?.IsAuthenticated ?? false,
            context.GetCorrelationId(),
            exception.GetType().FullName,
            exception.Message);
    }

    private static string GetErrorCategory(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation",
            ArgumentException or InvalidOperationException => "Validation",
            KeyNotFoundException => "NotFound",
            UnauthorizedAccessException => "Unauthorized",
            OperationCanceledException => "Cancelled",
            DbUpdateException => "Validation",
            _ => "Server"
        };

    private static (string? UserId, string? UserEmail) GetUserContext(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return (null, null);

        var sub = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = context.User.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? context.User.FindFirstValue(ClaimTypes.Email);
        return (sub, email);
    }

    private MappedError MapException(Exception exception) =>
        exception switch
        {
            ValidationException => new MappedError(
                StatusCodes.Status400BadRequest,
                "One or more validation rules failed."),

            OperationCanceledException => new MappedError(
                StatusCodes.Status408RequestTimeout,
                "The request was cancelled or timed out."),

            UnauthorizedAccessException => new MappedError(
                StatusCodes.Status401Unauthorized,
                exception.Message),

            KeyNotFoundException => new MappedError(
                StatusCodes.Status404NotFound,
                exception.Message),

            ArgumentException => new MappedError(
                StatusCodes.Status400BadRequest,
                exception.Message),

            InvalidOperationException => new MappedError(
                StatusCodes.Status400BadRequest,
                exception.Message),

            DbUpdateException => MapDbUpdateException(exception),

            _ => new MappedError(
                StatusCodes.Status500InternalServerError,
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.")
        };

    private MappedError MapDbUpdateException(Exception exception)
    {
        var message = "The data could not be saved. It may be invalid or conflict with existing records.";
        var details = _environment.IsDevelopment()
            ? exception.InnerException?.Message ?? exception.Message
            : null;

        return new MappedError(StatusCodes.Status400BadRequest, message, details);
    }

    private readonly record struct MappedError(int StatusCode, string Message, string? Details = null);
}
