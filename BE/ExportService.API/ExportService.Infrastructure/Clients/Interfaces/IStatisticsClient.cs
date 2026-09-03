using ExportService.DTO.Upstream;

namespace ExportService.Infrastructure.Clients.Interfaces;

public interface IStatisticsClient
{
    Task<IReadOnlyList<TimeseriesPoint>>  GetTimeseriesAsync(DateOnly from, DateOnly to, string granularity, CancellationToken ct = default);
    Task<IReadOnlyList<CategorySlice>>    GetCategoryBreakdownAsync(DateOnly from, DateOnly to, string kind, CancellationToken ct = default);
    Task<IReadOnlyList<TopCategory>>      GetTopCategoriesAsync(DateOnly from, DateOnly to, string kind, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<SavingsRatePoint>> GetSavingsRateAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
