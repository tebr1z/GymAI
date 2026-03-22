using FluentValidation;
using GymFit.Application.DTOs.Users;

namespace GymFit.Application.Validators;

public sealed class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName!).NotEmpty().MaximumLength(256);
        });

        When(x => x.Goal is not null, () =>
        {
            RuleFor(x => x.Goal!).MaximumLength(2000);
        });

        RuleFor(x => x.Weight)
            .InclusiveBetween(20, 500)
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Height)
            .InclusiveBetween(50, 300)
            .When(x => x.Height.HasValue);
    }
}
