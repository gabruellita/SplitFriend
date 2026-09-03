using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface IRecurringTemplateRepository
{
    Task<long> CreateAsync(long userId, long? categoryId, decimal amount, long currencyId, string kind,
                           string? description, string frequency, int intervalCount,
                           DateOnly startDate, DateOnly? endDate, DateOnly nextRunDate);
    Task<IEnumerable<RecurringTransactionTemplate>> GetAllAsync(long userId);
    Task<RecurringTransactionTemplate?> GetByIdAsync(long id, long userId);
    Task<int> UpdateAsync(long id, long userId, long? categoryId, decimal amount, long currencyId, string kind,
                          string? description, string frequency, int intervalCount, DateOnly? endDate);
    Task<int> DeactivateAsync(long id, long userId);
    Task<IEnumerable<RecurringTransactionTemplate>> GetDueAsync(long userId, DateOnly runDate);
    Task<IEnumerable<RecurringTransactionTemplate>> GetAllDueAsync(DateOnly runDate);
    Task AdvanceAsync(long id, DateOnly nextRunDate, bool isActive);
}
