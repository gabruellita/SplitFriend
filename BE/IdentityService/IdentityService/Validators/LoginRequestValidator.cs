using FluentValidation;
using IdentityService.DTO.Requests;

namespace IdentityService.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu.")
            .EmailAddress().WithMessage("Formatul email-ului este invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola este obligatorie.")
            .MinimumLength(8).WithMessage("Parola trebuie sa aiba minim 8 caractere.");
    }
}
