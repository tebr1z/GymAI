using GymFit.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GymFit.Api.Filters;

/// <summary>
/// Wraps successful <see cref="ObjectResult"/> payloads as <see cref="ApiResponse{T}"/>.
/// Skips <see cref="ProblemDetails"/>, bodies already using <see cref="IApiStandardEnvelope"/>, and typical 4xx object results.
/// </summary>
public sealed class ApiEnvelopeResultFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && ShouldWrap(objectResult))
        {
            var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
            if (statusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices
                && statusCode != StatusCodes.Status204NoContent)
            {
                objectResult.Value = ApiResponse<object>.Ok(objectResult.Value, "Success");
            }
        }

        return next();
    }

    private static bool ShouldWrap(ObjectResult objectResult)
    {
        if (objectResult.Value is ProblemDetails or ValidationProblemDetails)
            return false;
        if (objectResult.Value is IApiStandardEnvelope)
            return false;
        if (objectResult is BadRequestObjectResult or UnauthorizedObjectResult or NotFoundObjectResult)
            return false;

        return true;
    }
}
