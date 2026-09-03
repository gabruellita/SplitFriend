using System.Globalization;
using System.Net.Http.Json;
using ExportService.DTO.Upstream;
using ExportService.Infrastructure.Clients.Interfaces;
using ExportService.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace ExportService.Infrastructure.Clients;

public class StatisticsClient(HttpClient http, ILogger<StatisticsClient> logger) : IStatisticsClient
{
    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public Task<IReadOnlyList<TimeseriesPoint>> GetTimeseriesAsync(DateOnly from, DateOnly to, string granularity, CancellationToken ct = default)
        => GetListAsync<TimeseriesPoint>($"/api/statistics/timeseries?from={D(from)}&to={D(to)}&granularity={granularity}", ct);

    public Task<IReadOnlyList<CategorySlice>> GetCategoryBreakdownAsync(DateOnly from, DateOnly to, string kind, CancellationToken ct = default)
        => GetListAsync<CategorySlice>($"/api/statistics/category-breakdown?from={D(from)}&to={D(to)}&kind={kind}", ct);

    public Task<IReadOnlyList<TopCategory>> GetTopCategoriesAsync(DateOnly from, DateOnly to, string kind, int limit, CancellationToken ct = default)
        => GetListAsync<TopCategory>($"/api/statistics/top-categories?from={D(from)}&to={D(to)}&kind={kind}&limit={limit}", ct);

    public Task<IReadOnlyList<SavingsRatePoint>> GetSavingsRateAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => GetListAsync<SavingsRatePoint>($"/api/statistics/savings-rate?from={D(from)}&to={D(to)}", ct);

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var data = await http.GetFromJsonAsync<List<T>>(url, ct);
            return data ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Statistics indisponibil pentru {Url}", url);
            throw new ServiceUnavailableException("Serviciul de statistici este indisponibil. Porneste-l si reincearca.");
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Statistics timeout pentru {Url}", url);
            throw new ServiceUnavailableException("Serviciul de statistici nu a raspuns la timp. Reincearca.");
        }
    }
}
