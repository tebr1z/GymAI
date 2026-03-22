using FluentValidation;
using GymFit.Application.DTOs.AI;

namespace GymFit.Application.Validators;

public sealed class AiPlanRequestDtoValidator : AbstractValidator<AiPlanRequestDto>
{
    public AiPlanRequestDtoValidator()
    {
        RuleFor(x => x.UserInput).NotEmpty().MaximumLength(8000);
    }
}
