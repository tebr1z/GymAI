using FluentValidation;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Validators;

public sealed class TrainerMarketplaceQueryValidator : AbstractValidator<TrainerMarketplaceQuery>
{
    public TrainerMarketplaceQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, Pagination.MaxPageSize);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxPricePerMonth).GreaterThanOrEqualTo(0).When(x => x.MaxPricePerMonth.HasValue);
    }
}
