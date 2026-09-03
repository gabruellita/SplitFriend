using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceService.Services;

public class RecurringGenerationEngine(
    ITransactionRepository            txRepo,
    IRecurringTemplateRepository      templateRepo,
    ILogger<RecurringGenerationEngine> logger
) : IRecurringGenerationEngine
{
    public async Task<int> GenerateAsync(IEnumerable<RecurringTransactionTemplate> dueTemplates, DateOnly today)
    {
        var generated = 0;
        foreach (var t in dueTemplates)
        {
            var next = t.NextRunDate;
            while (next <= today && (t.EndDate is null || next <= t.EndDate))
            {
                await txRepo.CreateAsync(
                    t.UserId, t.CategoryId, t.Amount, t.CurrencyId, t.Kind, t.Description,
                    date: next, templateId: t.Id);
                generated++;
                next = ComputeNext(next, t.Frequency, t.IntervalCount);
            }

            var stillActive = t.EndDate is null || next <= t.EndDate;
            await templateRepo.AdvanceAsync(t.Id, next, stillActive);
        }

        // Log la nivel Debug: apelantii (RunDueAsync / job-ul) logheaza contul cu context (user / data).
        logger.LogDebug("Motor recurenta: {Count} tranzactii generate", generated);
        return generated;
    }

    private static DateOnly ComputeNext(DateOnly date, string frequency, int interval) => frequency switch
    {
        "DAILY"   => date.AddDays(interval),
        "WEEKLY"  => date.AddDays(7 * interval),
        "MONTHLY" => date.AddMonths(interval),
        "YEARLY"  => date.AddYears(interval),
        _         => throw new ValidationException($"Frecventa necunoscuta: {frequency}")
    };
}
