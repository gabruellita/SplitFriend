using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Numele grupului este obligatoriu.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Moneda este obligatorie.");
    }
}
