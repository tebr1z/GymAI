using FluentValidation;
using GymFit.Application.DTOs.Messages;
using GymFit.Application.Validation;

namespace GymFit.Application.Validators;

public sealed class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.ReceiverId).NotEmpty().WithMessage("ReceiverId is required.");

        RuleFor(x => x.MessageText)
            .NotEmptyOrWhitespace()
            .MaximumLength(8000).WithMessage("Message text must not exceed 8000 characters.");
    }
}
