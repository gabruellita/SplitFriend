using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class TransactionRepository(IDbConnectionFactory connectionFactory) : ITransactionRepository
{
    public async Task<long> CreateAsync(long userId, long? categoryId, decimal amount, long currencyId,
                                        string kind, string? description, DateOnly date, long? templateId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_transaction",
            new
            {
                p_user_id          = userId,
                p_category_id      = categoryId,
                p_amount           = amount,
                p_currency_id      = currencyId,
                p_kind             = kind,
                p_description      = description,
                p_transaction_date = date,
                p_template_id      = templateId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync(long userId, DateOnly? from, DateOnly? to,
                                                            long? categoryId, string? kind)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Transaction>(
            "sp_get_transactions",
            new { p_user_id = userId, p_from = from, p_to = to, p_category_id = categoryId, p_kind = kind },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Transaction?> GetByIdAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Transaction>(
            "sp_get_transaction_by_id",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(long id, long userId, long? categoryId, decimal amount, long currencyId,
                                       string kind, string? description, DateOnly date)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_update_transaction",
            new
            {
                p_id               = id,
                p_user_id          = userId,
                p_category_id      = categoryId,
                p_amount           = amount,
                p_currency_id      = currencyId,
                p_kind             = kind,
                p_description      = description,
                p_transaction_date = date
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> VoidAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_void_transaction",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SummaryRow>> GetSummaryAsync(long userId, DateOnly? from, DateOnly? to)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<SummaryRow>(
            "sp_get_summary",
            new { p_user_id = userId, p_from = from, p_to = to },
            commandType: CommandType.StoredProcedure);
    }
}
