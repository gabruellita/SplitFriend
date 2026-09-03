using FluentValidation;

namespace CurrencyService.API.Validators;

public record ConvertQuery(string From, string To, decimal Amount);

public class ConvertQueryValidator : AbstractValidator<ConvertQuery>
{
    public ConvertQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty().Matches("^[A-Za-z]{3}$").WithMessage("Cod de monedă invalid (3 litere).");
        RuleFor(x => x.To).NotEmpty().Matches("^[A-Za-z]{3}$").WithMessage("Cod de monedă invalid (3 litere).");
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
