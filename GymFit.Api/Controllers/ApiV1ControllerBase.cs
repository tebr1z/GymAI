using Asp.Versioning;
using GymFit.Api.Models;
using GymFit.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Controllers;

/// <summary>Default API surface version (1.0). Add v2+ by introducing <c>ApiV2ControllerBase</c> and new routes when needed.</summary>
[ApiController]
[ApiVersion("1.0")]
public abstract class ApiV1ControllerBase : ControllerBase
{
    protected ActionResult<T> MapToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ServiceFailure(result.Error, result.Kind);
    }

    protected IActionResult MapToActionResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return NoContent();

        return ServiceFailure(result.Error, result.Kind);
    }

    private ObjectResult ServiceFailure(string? message, ServiceFailureKind kind)
    {
        var status = (int)kind;
        var payload = new GlobalErrorResponse
        {
            Success = false,
            Message = string.IsNullOrWhiteSpace(message) ? "The request could not be completed." : message!,
            Details = null,
            Errors = null
        };

        return StatusCode(status, payload);
    }
}
