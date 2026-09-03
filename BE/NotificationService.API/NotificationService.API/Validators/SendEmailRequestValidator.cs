using FluentValidation;
using NotificationService.DTO;

namespace NotificationService.API.Validators;

public class SendEmailRequestValidator : AbstractValidator<SendEmailRequest>
{
    public SendEmailRequestValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Destinatarul este obligatoriu.")
            .EmailAddress().WithMessage("Email destinatar invalid.");

        RuleFor(x => x.Template)
            .NotEmpty().WithMessage("Template-ul este obligatoriu.");

        RuleFor(x => x.Data)
            .NotNull().WithMessage("Campul Data este obligatoriu.");
    }
}
