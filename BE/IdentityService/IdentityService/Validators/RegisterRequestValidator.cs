using FluentValidation;
using IdentityService.DTO.Requests;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IUserRepository userRepo, ICurrencyRepository currencyRepo)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu.")
            .EmailAddress().WithMessage("Formatul email-ului este invalid.")
            .MaximumLength(256).WithMessage("Email-ul nu poate depasi 256 de caractere.")
            .MustAsync(async (email, ct) =>
                !await userRepo.ExistsByEmailAsync(email.ToLowerInvariant().Trim()))
            .WithMessage("Email-ul este deja inregistrat.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username-ul este obligatoriu.")
            .MinimumLength(3).WithMessage("Username-ul trebuie sa aiba minim 3 caractere.")
            .MaximumLength(100).WithMessage("Username-ul nu poate depasi 100 de caractere.")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Doar litere, cifre, _ si -.")
            .MustAsync(async (username, ct) =>
                !await userRepo.ExistsByUsernameAsync(username.Trim()))
            .WithMessage("Username-ul este deja folosit.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola este obligatorie.")
            .MinimumLength(8).WithMessage("Parola trebuie sa aiba minim 8 caractere.")
            .MaximumLength(128).WithMessage("Parola nu poate depasi 128 de caractere.")
            .Matches(@"(?=.*[A-Z])").WithMessage("Necesita cel putin o litera mare.")
            .Matches(@"(?=.*[a-z])").WithMessage("Necesita cel putin o litera mica.")
            .Matches(@"(?=.*\d)").WithMessage("Necesita cel putin o cifra.")
            .Matches(@"(?=.*[\W_])").WithMessage("Necesita cel putin un caracter special.");

        RuleFor(x => x.PreferredCurrencyId)
            .GreaterThan(0).WithMessage("ID-ul monedei este invalid.")
            .MustAsync(async (id, ct) => await currencyRepo.ExistsActiveAsync(id))
            .WithMessage("Moneda selectata nu exista sau este inactiva.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Prenumele nu poate depasi 100 de caractere.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 de caractere.")
            .When(x => x.LastName is not null);
    }
}
