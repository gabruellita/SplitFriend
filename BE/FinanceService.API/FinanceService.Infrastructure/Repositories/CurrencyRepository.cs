using System.Data;
using Dapper;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

/// <summary>
/// Validare monede — reutilizeaza sp_currency_exists_active definit de Identity
/// in aceeasi baza finance_db (tabelul `currencies` e partajat, read-only aici).
/// </summary>
public class CurrencyRepository(IDbConnectionFactory connectionFactory) : ICurrencyRepository
{
    public async Task<bool> ExistsActiveAsync(long id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_currency_exists_active",
            new { p_id = id },
            commandType: CommandType.StoredProcedure);
    }
}
