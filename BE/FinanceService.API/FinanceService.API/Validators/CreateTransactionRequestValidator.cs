using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Suma trebuie sa fie pozitiva.");

        RuleFor(x => x.Kind)
            .Must(k => k is "INCOME" or "EXPENSE")
            .WithMessage("Tipul (kind) trebuie sa fie INCOME sau EXPENSE.");

        RuleFor(x => x.TransactionDate)
            .NotEqual(default(DateOnly)).WithMessage("Data tranzactiei este obligatorie.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId invalid.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).When(x => x.CurrencyId.HasValue)
            .WithMessage("CurrencyId invalid.");
    }
}
