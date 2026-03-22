using FluentValidation;

namespace GymFit.Application.Validation;

public static class PasswordValidationExtensions
{
    /// <summary>Strong password: length, upper, lower, digit, special character.</summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter (A–Z).")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter (a–z).")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character (non-letter, non-digit).");
}
