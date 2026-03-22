using FluentValidation;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Validators;

public sealed class BookTrainerDtoValidator : AbstractValidator<BookTrainerDto>
{
    public BookTrainerDtoValidator()
    {
        RuleFor(x => x.TrainerProfileId).NotEmpty();
        RuleFor(x => x.ExpectedPrice).GreaterThanOrEqualTo(0).When(x => x.ExpectedPrice.HasValue);
    }
}
