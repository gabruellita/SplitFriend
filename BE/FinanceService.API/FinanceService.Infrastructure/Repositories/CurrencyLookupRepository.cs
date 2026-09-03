using System.Data;
using Dapper;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class CurrencyLookupRepository(IDbConnectionFactory connectionFactory) : ICurrencyLookupRepository
{
    public async Task<string?> GetCodeAsync(long currencyId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<string?>(
            "sp_get_currency_code",
            new { p_id = currencyId },
            commandType: CommandType.StoredProcedure);
    }
}
