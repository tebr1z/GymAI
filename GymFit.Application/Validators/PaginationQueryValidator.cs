using FluentValidation;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Common;

namespace GymFit.Application.Validators;

public sealed class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, Pagination.MaxPageSize);
    }
}
