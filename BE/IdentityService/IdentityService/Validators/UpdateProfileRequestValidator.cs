using FluentValidation;
using IdentityService.DTO.Requests;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.API.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(ICurrencyRepository currencyRepo)
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Prenumele nu poate depasi 100 de caractere.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 de caractere.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.PreferredCurrencyId)
            .GreaterThan(0).WithMessage("ID-ul monedei este invalid.")
            .MustAsync(async (id, ct) => await currencyRepo.ExistsActiveAsync(id!.Value))
            .WithMessage("Moneda selectata nu exista sau este inactiva.")
            .When(x => x.PreferredCurrencyId is not null);
    }
}
