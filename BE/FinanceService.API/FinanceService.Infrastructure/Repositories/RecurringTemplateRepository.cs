using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class RecurringTemplateRepository(IDbConnectionFactory connectionFactory) : IRecurringTemplateRepository
{
    public async Task<long> CreateAsync(long userId, long? categoryId, decimal amount, long currencyId, string kind,
                                        string? description, string frequency, int intervalCount,
                                        DateOnly startDate, DateOnly? endDate, DateOnly nextRunDate)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_recurring_template",
            new
            {
                p_user_id        = userId,
                p_category_id    = categoryId,
                p_amount         = amount,
                p_currency_id    = currencyId,
                p_kind           = kind,
                p_description    = description,
                p_frequency      = frequency,
                p_interval_count = intervalCount,
                p_start_date     = startDate,
                p_end_date       = endDate,
                p_next_run_date  = nextRunDate
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RecurringTransactionTemplate>> GetAllAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RecurringTransactionTemplate>(
            "sp_get_recurring_templates",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<RecurringTransactionTemplate?> GetByIdAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<RecurringTransactionTemplate>(
            "sp_get_recurring_template_by_id",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(long id, long userId, long? categoryId, decimal amount, long currencyId,
                                       string kind, string? description, string frequency, int intervalCount,
                                       DateOnly? endDate)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_update_recurring_template",
            new
            {
                p_id             = id,
                p_user_id        = userId,
                p_category_id    = categoryId,
                p_amount         = amount,
                p_currency_id    = currencyId,
                p_kind           = kind,
                p_description    = description,
                p_frequency      = frequency,
                p_interval_count = intervalCount,
                p_end_date       = endDate
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> DeactivateAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_deactivate_recurring_template",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RecurringTransactionTemplate>> GetDueAsync(long userId, DateOnly runDate)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RecurringTransactionTemplate>(
            "sp_get_due_templates",
            new { p_user_id = userId, p_run_date = runDate },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RecurringTransactionTemplate>> GetAllDueAsync(DateOnly runDate)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RecurringTransactionTemplate>(
            "sp_get_all_due_templates",
            new { p_run_date = runDate },
            commandType: CommandType.StoredProcedure);
    }

    public async Task AdvanceAsync(long id, DateOnly nextRunDate, bool isActive)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_advance_template",
            new { p_id = id, p_next_run_date = nextRunDate, p_is_active = isActive },
            commandType: CommandType.StoredProcedure);
    }
}
