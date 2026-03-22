using FluentValidation;
using GymFit.Application.DTOs.Trainers;

namespace GymFit.Application.Validators;

public sealed class BookTrainerDtoValidator : AbstractValidator<BookTrainerDto>
{
    private const decimal MaxPrice = 1_000_000m;

    public BookTrainerDtoValidator()
    {
        RuleFor(x => x.TrainerProfileId).NotEmpty().WithMessage("TrainerProfileId is required.");

        RuleFor(x => x.ExpectedPrice)
            .InclusiveBetween(0, MaxPrice)
            .When(x => x.ExpectedPrice.HasValue)
            .WithMessage($"Expected price must be between 0 and {MaxPrice:N0}.");
    }
}
