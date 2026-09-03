namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface ICurrencyLookupRepository
{
    Task<string?> GetCodeAsync(long currencyId);
}
