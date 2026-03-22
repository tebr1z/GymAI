using FluentValidation;

namespace GymFit.Application.Validation;

public static class EmailValidationExtensions
{
    public static IRuleBuilderOptions<T, string> StrictEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Email must be a valid address.")
            .Must(e => e == e.Trim()).WithMessage("Email must not have leading or trailing spaces.")
            .Must(e => !e.Contains(' ', StringComparison.Ordinal)).WithMessage("Email cannot contain spaces.");
}
