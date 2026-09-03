using StatisticsService.DTO.Responses;

namespace StatisticsService.Services.Interfaces;

public interface IStatsService
{
    Task<IReadOnlyList<TimeseriesPointDto>>  TimeseriesAsync(DateOnly? from, DateOnly? to, string? granularity);
    Task<IReadOnlyList<CategorySliceDto>>    CategoryBreakdownAsync(DateOnly? from, DateOnly? to, string kind);
    Task<IReadOnlyList<TopCategoryDto>>      TopCategoriesAsync(DateOnly? from, DateOnly? to, string kind, int? limit);
    Task<IReadOnlyList<CalendarDayDto>>      CalendarAsync(DateOnly? from, DateOnly? to);
    Task<IReadOnlyList<HistogramBucketDto>>  HistogramAsync(DateOnly? from, DateOnly? to, decimal? max, int? buckets);
    Task<IReadOnlyList<SavingsRateDto>>      SavingsRateAsync(DateOnly? from, DateOnly? to);
    Task<IReadOnlyList<RunningBalanceDto>>   RunningBalanceAsync(DateOnly? from, DateOnly? to);
    Task<IReadOnlyList<MoMPointDto>>         MoMAsync(DateOnly? from, DateOnly? to, string kind, string? granularity);
    Task<IReadOnlyList<ParetoSliceDto>>      ParetoAsync(DateOnly? from, DateOnly? to);
    Task<IReadOnlyList<WeekdayDto>>          WeekdayAsync(DateOnly? from, DateOnly? to, string kind);
    Task<IReadOnlyList<RecurringSplitDto>>   RecurringSplitAsync(DateOnly? from, DateOnly? to, string kind);
}
