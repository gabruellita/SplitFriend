using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class GroupExpenseRepository(IDbConnectionFactory connectionFactory) : IGroupExpenseRepository
{
    public async Task<long> CreateAsync(long groupId, long paidBy, string title, decimal amount, long currencyId,
                                        string splitType, DateOnly expenseDate, string splitsJson)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_group_expense",
            new
            {
                p_group_id     = groupId,
                p_paid_by      = paidBy,
                p_title        = title,
                p_amount       = amount,
                p_currency_id  = currencyId,
                p_split_type   = splitType,
                p_expense_date = expenseDate,
                p_splits       = new JsonbParameter(splitsJson)
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<GroupExpense>> GetAllAsync(long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<GroupExpense>(
            "sp_get_group_expenses",
            new { p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<GroupExpense?> GetByIdAsync(long id, long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<GroupExpense>(
            "sp_get_group_expense_by_id",
            new { p_id = id, p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ExpenseSplit>> GetSplitsAsync(long expenseId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<ExpenseSplit>(
            "sp_get_expense_splits",
            new { p_expense_id = expenseId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CancelAsync(long id, long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_cancel_group_expense",
            new { p_id = id, p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<GroupBalanceRow>> GetBalancesAsync(long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<GroupBalanceRow>(
            "sp_get_group_balances",
            new { p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }
}
