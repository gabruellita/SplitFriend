namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface ICurrencyRepository
{
    Task<bool> ExistsActiveAsync(long id);
}
