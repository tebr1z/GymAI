using FluentValidation;
using GymFit.Application.DTOs.AI;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class AiPlanRequestDtoValidator : AbstractValidator<AiPlanRequestDto>
{
    public AiPlanRequestDtoValidator()
    {
        RuleFor(x => x.UserInput)
            .NotEmptyOrWhitespace()
            .MinimumLength(3).WithMessage("Please enter at least 3 characters so the AI can help.")
            .MaximumLength(8000)
            .WithMessage("Input must not exceed 8000 characters.");
    }
}
