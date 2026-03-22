using FluentValidation;
using GymFit.Application.DTOs.Messages;

namespace GymFit.Application.Validators;

public sealed class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.ReceiverId).NotEmpty();
        RuleFor(x => x.MessageText).NotEmpty().MaximumLength(8000);
    }
}
