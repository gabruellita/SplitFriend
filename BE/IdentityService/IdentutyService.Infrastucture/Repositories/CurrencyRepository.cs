using System.Data;
using Dapper;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class CurrencyRepository(IDbConnectionFactory connectionFactory) : ICurrencyRepository
{
    public async Task<IEnumerable<Currency>> GetAllActiveAsync()
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Currency>(
            "sp_get_active_currencies",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ExistsActiveAsync(long id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_currency_exists_active",
            new { p_id = id },
            commandType: CommandType.StoredProcedure);
    }
}
