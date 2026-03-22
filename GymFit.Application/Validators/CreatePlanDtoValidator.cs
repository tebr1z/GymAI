using FluentValidation;
using GymFit.Application.DTOs.Plans;

namespace GymFit.Application.Validators;

public sealed class CreatePlanDtoValidator : AbstractValidator<CreatePlanDto>
{
    public CreatePlanDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(100_000);
    }
}
