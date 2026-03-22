using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GymFit.Infrastructure.Data;

/// <summary>
/// Applies EF Core migrations at startup without tearing down the web host if PostgreSQL is unreachable or misconfigured.
/// </summary>
public static class DatabaseInitializer
{
    public const string UserFacingFailureMessage = "Database connection failed";

    public static async Task<DatabaseInitializationResult> TryMigrateAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("PostgreSQL database is ready and migrations are up to date.");
            return DatabaseInitializationResult.Succeeded();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (NpgsqlException ex)
        {
            var detail = ClassifyNpgsql(ex);
            logger.LogError(
                ex,
                "{UserMessage}. {Detail} SqlState: {SqlState}, Message: {Message}",
                UserFacingFailureMessage,
                detail,
                ex.SqlState,
                ex.Message);
            return DatabaseInitializationResult.Failed($"{UserFacingFailureMessage}. {detail}");
        }
        catch (TimeoutException ex)
        {
            logger.LogError(
                ex,
                "{UserMessage}. The server did not respond in time (connection or command timeout).",
                UserFacingFailureMessage);
            return DatabaseInitializationResult.Failed(
                $"{UserFacingFailureMessage}. Connection or operation timed out.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{UserMessage}. Unexpected error while applying migrations.",
                UserFacingFailureMessage);
            return DatabaseInitializationResult.Failed($"{UserFacingFailureMessage}. See logs for details.");
        }
    }

    private static string ClassifyNpgsql(NpgsqlException ex) =>
        ex.SqlState switch
        {
            "28P01" => "Invalid database credentials (authentication failed).",
            "3D000" => "The database does not exist. Create the database or fix the connection string.",
            "57P03" => "The database system is starting up or not accepting connections yet.",
            "08006" => "Connection failure (network, host, or port unreachable).",
            _ when ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) =>
                "Connection timed out. Check host, port, firewall, and PostgreSQL max_connections.",
            _ => "See PostgreSQL error message for details."
        };
}

public readonly record struct DatabaseInitializationResult(bool Success, string? Message)
{
    public static DatabaseInitializationResult Succeeded() => new(true, null);

    public static DatabaseInitializationResult Failed(string message) => new(false, message);
}
