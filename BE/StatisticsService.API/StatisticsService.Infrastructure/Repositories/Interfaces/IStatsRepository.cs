using StatisticsService.Infrastructure.Models;

namespace StatisticsService.Infrastructure.Repositories.Interfaces;

public interface IStatsRepository
{
    Task<IEnumerable<TimeseriesRow>>        TimeseriesAsync(long userId, DateOnly? from, DateOnly? to, string granularity);
    Task<IEnumerable<CategoryBreakdownRow>> CategoryBreakdownAsync(long userId, DateOnly? from, DateOnly? to, string kind);
    Task<IEnumerable<TopCategoryRow>>       TopCategoriesAsync(long userId, DateOnly? from, DateOnly? to, string kind, int limit);
    Task<IEnumerable<CalendarRow>>          CalendarAsync(long userId, DateOnly from, DateOnly to);
    Task<IEnumerable<HistogramRow>>         HistogramAsync(long userId, DateOnly? from, DateOnly? to, decimal max, int buckets);
    Task<IEnumerable<SavingsRateRow>>       SavingsRateAsync(long userId, DateOnly? from, DateOnly? to);
    Task<IEnumerable<RunningBalanceRow>>    RunningBalanceAsync(long userId, DateOnly? from, DateOnly? to);
    Task<IEnumerable<MoMRow>>               MoMAsync(long userId, DateOnly? from, DateOnly? to, string kind, string granularity);
    Task<IEnumerable<ParetoRow>>            ParetoAsync(long userId, DateOnly? from, DateOnly? to);
    Task<IEnumerable<WeekdayRow>>           WeekdayAsync(long userId, DateOnly? from, DateOnly? to, string kind);
    Task<IEnumerable<RecurringSplitRow>>    RecurringSplitAsync(long userId, DateOnly? from, DateOnly? to, string kind);
}
