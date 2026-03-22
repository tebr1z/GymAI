using FluentValidation;
using GymFit.Application.Common;
using GymFit.Application.DTOs.Messages;

namespace GymFit.Application.Validators;

public sealed class MessagePaginationQueryValidator : AbstractValidator<MessagePaginationQuery>
{
    public MessagePaginationQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, Pagination.MaxPageSize);
    }
}
