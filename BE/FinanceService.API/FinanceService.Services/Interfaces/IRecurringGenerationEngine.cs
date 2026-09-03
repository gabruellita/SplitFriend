using FinanceService.Infrastructure.Models;

namespace FinanceService.Services.Interfaces;

public interface IRecurringGenerationEngine
{
    /// <summary>
    /// Genereaza tranzactiile scadente pentru template-urile date si avanseaza next_run_date.
    /// Nu depinde de ICurrentUser — fiecare template poarta propriul UserId. Returneaza cate s-au generat.
    /// </summary>
    Task<int> GenerateAsync(IEnumerable<RecurringTransactionTemplate> dueTemplates, DateOnly today);
}
