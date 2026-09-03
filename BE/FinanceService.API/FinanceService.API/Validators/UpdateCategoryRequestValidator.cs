using FinanceService.DTO.Requests;
using FluentValidation;

namespace FinanceService.API.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Numele categoriei este obligatoriu.")
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 de caractere.");

        RuleFor(x => x.Icon)
            .MaximumLength(50).When(x => x.Icon is not null);

        RuleFor(x => x.Color)
            .MaximumLength(20).When(x => x.Color is not null);
    }
}
