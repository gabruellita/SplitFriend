using System.Data;
using Dapper;
using StatisticsService.Infrastructure.Models;
using StatisticsService.Infrastructure.Repositories.Interfaces;

namespace StatisticsService.Infrastructure.Repositories;

public class StatsRepository(IDbConnectionFactory connectionFactory) : IStatsRepository
{
    public async Task<IEnumerable<TimeseriesRow>> TimeseriesAsync(long userId, DateOnly? from, DateOnly? to, string granularity)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<TimeseriesRow>(
            "sp_stats_timeseries",
            new { p_user_id = userId, p_from = from, p_to = to, p_granularity = granularity },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CategoryBreakdownRow>> CategoryBreakdownAsync(long userId, DateOnly? from, DateOnly? to, string kind)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<CategoryBreakdownRow>(
            "sp_stats_category_breakdown",
            new { p_user_id = userId, p_from = from, p_to = to, p_kind = kind },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TopCategoryRow>> TopCategoriesAsync(long userId, DateOnly? from, DateOnly? to, string kind, int limit)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<TopCategoryRow>(
            "sp_stats_top_categories",
            new { p_user_id = userId, p_from = from, p_to = to, p_kind = kind, p_limit = limit },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CalendarRow>> CalendarAsync(long userId, DateOnly from, DateOnly to)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<CalendarRow>(
            "sp_stats_calendar",
            new { p_user_id = userId, p_from = from, p_to = to },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<HistogramRow>> HistogramAsync(long userId, DateOnly? from, DateOnly? to, decimal max, int buckets)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<HistogramRow>(
            "sp_stats_histogram",
            new { p_user_id = userId, p_from = from, p_to = to, p_max = max, p_buckets = buckets },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SavingsRateRow>> SavingsRateAsync(long userId, DateOnly? from, DateOnly? to)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<SavingsRateRow>(
            "sp_stats_savings_rate",
            new { p_user_id = userId, p_from = from, p_to = to },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RunningBalanceRow>> RunningBalanceAsync(long userId, DateOnly? from, DateOnly? to)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RunningBalanceRow>(
            "sp_stats_running_balance",
            new { p_user_id = userId, p_from = from, p_to = to },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<MoMRow>> MoMAsync(long userId, DateOnly? from, DateOnly? to, string kind, string granularity)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<MoMRow>(
            "sp_stats_mom",
            new { p_user_id = userId, p_from = from, p_to = to, p_kind = kind, p_granularity = granularity },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ParetoRow>> ParetoAsync(long userId, DateOnly? from, DateOnly? to)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<ParetoRow>(
            "sp_stats_pareto",
            new { p_user_id = userId, p_from = from, p_to = to },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<WeekdayRow>> WeekdayAsync(long userId, DateOnly? from, DateOnly? to, string kind)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<WeekdayRow>(
            "sp_stats_weekday",
            new { p_user_id = userId, p_from = from, p_to = to, p_kind = kind },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RecurringSplitRow>> RecurringSplitAsync(long userId, DateOnly? from, DateOnly? to, string kind)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<RecurringSplitRow>(
            "sp_stats_recurring_split",
            new { p_user_id = userId, p_from = from, p_to = to, p_kind = kind },
            commandType: CommandType.StoredProcedure);
    }
}
