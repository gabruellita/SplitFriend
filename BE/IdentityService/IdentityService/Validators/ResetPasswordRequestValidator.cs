using FluentValidation;
using IdentityService.DTO.Requests;

namespace IdentityService.API.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Tokenul este obligatoriu.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Parola este obligatorie.")
            .MinimumLength(8).WithMessage("Parola trebuie sa aiba minim 8 caractere.")
            .MaximumLength(128).WithMessage("Parola nu poate depasi 128 de caractere.")
            .Matches(@"(?=.*[A-Z])").WithMessage("Necesita cel putin o litera mare.")
            .Matches(@"(?=.*[a-z])").WithMessage("Necesita cel putin o litera mica.")
            .Matches(@"(?=.*\d)").WithMessage("Necesita cel putin o cifra.")
            .Matches(@"(?=.*[\W_])").WithMessage("Necesita cel putin un caracter special.");
    }
}
