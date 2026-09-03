using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreateRecurringTemplateRequestValidator : AbstractValidator<CreateRecurringTemplateRequest>
{
    private static readonly string[] Frequencies = ["DAILY", "WEEKLY", "MONTHLY", "YEARLY"];

    public CreateRecurringTemplateRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Suma trebuie sa fie pozitiva.");

        RuleFor(x => x.Kind)
            .Must(k => k is "INCOME" or "EXPENSE")
            .WithMessage("Tipul (kind) trebuie sa fie INCOME sau EXPENSE.");

        RuleFor(x => x.Frequency)
            .Must(f => Frequencies.Contains(f))
            .WithMessage("Frecventa trebuie sa fie DAILY, WEEKLY, MONTHLY sau YEARLY.");

        RuleFor(x => x.IntervalCount)
            .GreaterThan(0).WithMessage("interval_count trebuie sa fie cel putin 1.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly)).WithMessage("start_date este obligatorie.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("end_date trebuie sa fie >= start_date.")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue);

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).When(x => x.CurrencyId.HasValue);
    }
}
