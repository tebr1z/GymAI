using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymFit.Application.Common;

/// <summary>
/// Runs service logic, maps database and unexpected failures to <see cref="ServiceResult{T}"/>,
/// logs full exceptions, and rethrows only <see cref="OperationCanceledException"/>.
/// </summary>
public static class ServiceExecution
{
    public static async Task<ServiceResult<T>> RunAsync<T>(
        ILogger logger,
        string operationName,
        Func<Task<ServiceResult<T>>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database failure during {Operation}.", operationName);
            return ServiceResult<T>.Fail(
                "We could not save your changes. Please verify your data and try again.",
                ServiceFailureKind.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure during {Operation}.", operationName);
            return ServiceResult<T>.Fail(
                "Something went wrong while processing your request. Please try again.",
                ServiceFailureKind.BadRequest);
        }
    }

    public static async Task<ServiceResult> RunAsync(
        ILogger logger,
        string operationName,
        Func<Task<ServiceResult>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database failure during {Operation}.", operationName);
            return ServiceResult.Fail(
                "We could not save your changes. Please verify your data and try again.",
                ServiceFailureKind.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure during {Operation}.", operationName);
            return ServiceResult.Fail(
                "Something went wrong while processing your request. Please try again.",
                ServiceFailureKind.BadRequest);
        }
    }
}
