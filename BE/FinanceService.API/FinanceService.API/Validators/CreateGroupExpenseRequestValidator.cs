using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class CreateGroupExpenseRequestValidator : AbstractValidator<CreateGroupExpenseRequest>
{
    private static readonly string[] ValidSplitTypes = ["EQUAL", "EXACT", "PERCENT", "SHARES"];

    public CreateGroupExpenseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Titlul este obligatoriu.")
            .MaximumLength(200);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Suma trebuie sa fie pozitiva.");

        RuleFor(x => x.PaidByUserId)
            .GreaterThan(0).WithMessage("Platitorul este obligatoriu.");

        RuleFor(x => x.SplitType)
            .Must(t => ValidSplitTypes.Contains(t))
            .WithMessage("Tipul de split trebuie sa fie EQUAL, EXACT, PERCENT sau SHARES.");

        RuleFor(x => x.ExpenseDate)
            .NotEqual(default(DateOnly)).WithMessage("Data cheltuielii este obligatorie.");

        RuleFor(x => x.Participants)
            .NotEmpty().WithMessage("Trebuie cel putin un participant.");

        RuleForEach(x => x.Participants).ChildRules(p =>
            p.RuleFor(pp => pp.UserId).GreaterThan(0).WithMessage("UserId participant invalid."));
    }
}
