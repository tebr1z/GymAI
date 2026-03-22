using FluentValidation;
using GymFit.Application.DTOs.Users;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x).Custom((dto, context) =>
        {
            if (dto.FullName is null && !dto.Weight.HasValue && !dto.Height.HasValue && dto.Goal is null)
                context.AddFailure("profile", "At least one of fullName, weight, height, or goal must be provided.");
        });

        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName!).NotEmptyOrWhitespace().MaximumLength(256);
        });

        When(x => x.Goal is not null, () =>
        {
            RuleFor(x => x.Goal!).MaximumLength(2000);
        });

        RuleFor(x => x.Weight)
            .InclusiveBetween(20, 500)
            .When(x => x.Weight.HasValue)
            .WithMessage("Weight must be between 20 and 500 (kg).");

        RuleFor(x => x.Height)
            .InclusiveBetween(50, 300)
            .When(x => x.Height.HasValue)
            .WithMessage("Height must be between 50 and 300 (cm).");
    }
}
