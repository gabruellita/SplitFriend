using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Numele categoriei este obligatoriu.")
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 de caractere.");

        RuleFor(x => x.Kind)
            .Must(k => k is "INCOME" or "EXPENSE")
            .WithMessage("Tipul (kind) trebuie sa fie INCOME sau EXPENSE.");

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon-ul nu poate depasi 50 de caractere.")
            .When(x => x.Icon is not null);

        RuleFor(x => x.Color)
            .MaximumLength(20).WithMessage("Culoarea nu poate depasi 20 de caractere.")
            .When(x => x.Color is not null);
    }
}
