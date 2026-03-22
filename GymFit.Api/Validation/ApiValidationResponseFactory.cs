using GymFit.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Validation;

public static class ApiValidationResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var fieldErrors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => string.IsNullOrEmpty(x.Key) ? "_" : x.Key,
                x => x.Value!.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var details = string.Join(
            "; ",
            fieldErrors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}")));

        var payload = new GlobalErrorResponse
        {
            Success = false,
            Message = "Validation failed. See details for field-specific errors.",
            Details = string.IsNullOrEmpty(details) ? null : details
        };

        return new BadRequestObjectResult(payload);
    }
}
