using FluentValidation;

namespace GymFit.Application.Validation;

public static class StringValidationExtensions
{
    /// <summary>Non-null, non-empty, and not whitespace-only.</summary>
    public static IRuleBuilderOptions<T, string> NotEmptyOrWhitespace<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotNull().WithMessage("This field is required.")
            .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("This field cannot be empty or whitespace only.");
}
