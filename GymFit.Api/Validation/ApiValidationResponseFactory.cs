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
                x => ToCamelCaseKey(string.IsNullOrEmpty(x.Key) ? "_" : x.Key),
                x => x.Value!.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    .ToArray(),
                StringComparer.Ordinal);

        var details = string.Join(
            "; ",
            fieldErrors.SelectMany(kv => kv.Value.Select(msg => $"{kv.Key}: {msg}")));

        var payload = new GlobalErrorResponse
        {
            Success = false,
            Message = "Validation failed. See errors for field-specific messages.",
            Details = string.IsNullOrEmpty(details) ? null : details,
            Errors = fieldErrors.Count > 0 ? fieldErrors : null
        };

        return new BadRequestObjectResult(payload);
    }

    private static string ToCamelCaseKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key == "_")
            return key;
        if (!char.IsUpper(key[0]))
            return key;
        return char.ToLowerInvariant(key[0]) + key.Substring(1);
    }
}
