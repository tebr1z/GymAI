using FluentValidation;
using GymFit.Application.DTOs.Plans;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class CreatePlanDtoValidator : AbstractValidator<CreatePlanDto>
{
    public CreatePlanDtoValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Plan type is not a supported value.");

        RuleFor(x => x.Content)
            .NotEmptyOrWhitespace()
            .MaximumLength(100_000)
            .Must(BeValidJsonObject)
            .WithMessage("Content must be a valid JSON object or array.");

        RuleFor(x => x.TrainerId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("TrainerId cannot be an empty GUID.");
    }

    private static bool BeValidJsonObject(string content)
    {
        var t = content.Trim();
        if (t.Length < 2)
            return false;
        if (t[0] == '{' && t[^1] == '}')
            return true;
        if (t[0] == '[' && t[^1] == ']')
            return true;
        return false;
    }
}
