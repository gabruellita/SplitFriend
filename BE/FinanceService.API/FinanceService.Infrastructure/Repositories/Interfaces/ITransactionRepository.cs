using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<long> CreateAsync(long userId, long? categoryId, decimal amount, long currencyId,
                           string kind, string? description, DateOnly date, long? templateId);
    Task<IEnumerable<Transaction>> GetAllAsync(long userId, DateOnly? from, DateOnly? to,
                                               long? categoryId, string? kind);
    Task<Transaction?> GetByIdAsync(long id, long userId);
    Task<int> UpdateAsync(long id, long userId, long? categoryId, decimal amount, long currencyId,
                          string kind, string? description, DateOnly date);
    Task<int> VoidAsync(long id, long userId);
    Task<IEnumerable<SummaryRow>> GetSummaryAsync(long userId, DateOnly? from, DateOnly? to);
}
