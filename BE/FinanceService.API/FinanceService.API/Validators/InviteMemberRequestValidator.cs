using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class InviteMemberRequestValidator : AbstractValidator<InviteMemberRequest>
{
    public InviteMemberRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu.")
            .EmailAddress().WithMessage("Email invalid.");
    }
}
