using FluentValidation;
using GymFit.Application.DTOs.Auth;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email).StrictEmail();

        RuleFor(x => x.Password).StrongPassword();

        RuleFor(x => x.FullName)
            .NotEmptyOrWhitespace()
            .MaximumLength(256)
            .Must(n => n.Trim().Length >= 2)
            .WithMessage("Full name must be at least 2 characters.")
            .Matches(@"^[\p{L}\p{M}\s'.-]+$")
            .WithMessage("Full name may only contain letters, spaces, apostrophes, periods, and hyphens.");
    }
}
