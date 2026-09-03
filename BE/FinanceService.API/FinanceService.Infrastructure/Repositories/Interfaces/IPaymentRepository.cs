using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public record CreditorCurrency(long CurrencyId, string CurrencyCode, decimal RemainingOwed);

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync(long groupId);
    Task<CreditorCurrency?> GetCreditorCurrencyAsync(long groupId, long fromUser, long toUser);
    Task<long> CreateAsync(
        long groupId, long fromUser, long toUser,
        decimal amount, long currencyId,
        decimal originalAmount, long originalCurrencyId,
        decimal exchangeRate, DateOnly rateDate, string? method);
}
