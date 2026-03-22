using FluentValidation;
using GymFit.Application.DTOs.Subscriptions;

namespace GymFit.Application.Validators;

public sealed class AssignSubscriptionDtoValidator : AbstractValidator<AssignSubscriptionDto>
{
    public AssignSubscriptionDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Tier).IsInEnum().WithMessage("Tier must be a valid subscription tier.");

        RuleFor(x => x.EndDateUtc)
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("End date must be in the future (UTC).")
            .Must(d => d <= DateTime.UtcNow.AddYears(20))
            .WithMessage("End date cannot be more than 20 years in the future.");

        RuleFor(x => x.ExternalProvider).MaximumLength(64).When(x => x.ExternalProvider is not null);
        RuleFor(x => x.ExternalCustomerId).MaximumLength(256).When(x => x.ExternalCustomerId is not null);
        RuleFor(x => x.ExternalSubscriptionId).MaximumLength(256).When(x => x.ExternalSubscriptionId is not null);
    }
}
