using GymFit.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Api.Validation;

public static class ApiValidationResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var fieldErrors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x =>
            {
                var key = ToCamelCaseKey(string.IsNullOrEmpty(x.Key) ? "_" : x.Key);
                return x.Value!.Errors.Select(e =>
                {
                    var msg = string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage;
                    return key == "_" ? msg : $"{key}: {msg}";
                });
            })
            .ToList();

        var payload = ApiResponse<object?>.Fail(
            "Validation failed. See errors for details.",
            fieldErrors);

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
