using FluentValidation;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Validators;

public sealed class TrainerMarketplaceQueryValidator : AbstractValidator<TrainerMarketplaceQuery>
{
    private const decimal MaxPriceFilter = 10_000_000m;

    public TrainerMarketplaceQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, Pagination.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {Pagination.MaxPageSize}.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("MinRating must be between 0 and 5.");

        RuleFor(x => x.MaxPricePerMonth)
            .InclusiveBetween(0, MaxPriceFilter)
            .When(x => x.MaxPricePerMonth.HasValue)
            .WithMessage($"MaxPricePerMonth must be between 0 and {MaxPriceFilter:N0}.");
    }
}
