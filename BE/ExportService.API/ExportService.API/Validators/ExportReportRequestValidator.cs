using System.Text.RegularExpressions;
using ExportService.DTO.Requests;
using FluentValidation;

namespace ExportService.API.Validators;

public partial class ExportReportRequestValidator : AbstractValidator<ExportReportRequest>
{
    private static readonly string[] ValidBlocks = ["SUMMARY", "TREND", "CATEGORIES", "TRANSACTIONS"];

    public ExportReportRequestValidator()
    {
        RuleFor(x => x.Mode)
            .Must(m => m is "MONTHS" or "RANGE")
            .WithMessage("Mode trebuie sa fie MONTHS sau RANGE.");

        RuleFor(x => x.Blocks)
            .NotEmpty().WithMessage("Selecteaza cel putin un bloc de continut.")
            .Must(b => b.All(x => ValidBlocks.Contains(x.ToUpperInvariant())))
            .WithMessage("Blocuri permise: SUMMARY, TREND, CATEGORIES, TRANSACTIONS.");

        When(x => x.Mode == "MONTHS", () =>
        {
            RuleFor(x => x.Months)
                .NotNull().NotEmpty().WithMessage("Selecteaza cel putin o luna.");
            RuleFor(x => x.Months!)
                .Must(ms => ms.Count <= 24).WithMessage("Maxim 24 de luni per raport.")
                .Must(ms => ms.All(m => MonthRegex().IsMatch(m)))
                .WithMessage("Lunile trebuie in format YYYY-MM.");
        });

        When(x => x.Mode == "RANGE", () =>
        {
            RuleFor(x => x.Range).NotNull().WithMessage("Range obligatoriu la mode RANGE.");
            RuleFor(x => x.Range!).Must(r => r.From <= r.To)
                .WithMessage("Data de inceput trebuie sa fie <= data de final.");
        });
    }

    [GeneratedRegex(@"^\d{4}-\d{2}$")]
    private static partial Regex MonthRegex();
}
