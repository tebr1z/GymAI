using FluentValidation;
using GymFit.Application.DTOs.Auth;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).StrictEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.");
    }
}
