using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface IGroupExpenseRepository
{
    Task<long> CreateAsync(long groupId, long paidBy, string title, decimal amount, long currencyId,
                           string splitType, DateOnly expenseDate, string splitsJson);
    Task<IEnumerable<GroupExpense>> GetAllAsync(long groupId);
    Task<GroupExpense?> GetByIdAsync(long id, long groupId);
    Task<IEnumerable<ExpenseSplit>> GetSplitsAsync(long expenseId);
    Task<int> CancelAsync(long id, long groupId);
    Task<IEnumerable<GroupBalanceRow>> GetBalancesAsync(long groupId);
}
