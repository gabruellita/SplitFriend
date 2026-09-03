using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.ToUserId)
            .GreaterThan(0).WithMessage("Destinatarul platii este obligatoriu.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Suma platii trebuie sa fie pozitiva.");

        RuleFor(x => x.Method)
            .MaximumLength(50).When(x => x.Method is not null);
    }
}
