using System.Globalization;
using System.Net.Http.Json;
using ExportService.DTO.Upstream;
using ExportService.Infrastructure.Clients.Interfaces;
using ExportService.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace ExportService.Infrastructure.Clients;

public class FinanceClient(HttpClient http, ILogger<FinanceClient> logger) : IFinanceClient
{
    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public async Task<IReadOnlyList<FinanceTransaction>> GetTransactionsAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        try
        {
            var data = await http.GetFromJsonAsync<List<FinanceTransaction>>(
                $"/api/transactions?from={D(from)}&to={D(to)}", ct);
            return data ?? [];
        }
        catch (HttpRequestException ex)            { throw Unavailable(ex); }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested) { throw Unavailable(ex); }
    }

    public async Task<FinanceSummary> GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        try
        {
            var data = await http.GetFromJsonAsync<FinanceSummary>(
                $"/api/transactions/summary?from={D(from)}&to={D(to)}", ct);
            return data ?? new FinanceSummary(0, 0, 0, []);
        }
        catch (HttpRequestException ex)            { throw Unavailable(ex); }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested) { throw Unavailable(ex); }
    }

    private ServiceUnavailableException Unavailable(Exception ex)
    {
        logger.LogWarning(ex, "Finance indisponibil");
        return new ServiceUnavailableException("Serviciul Finance este indisponibil. Porneste-l si reincearca.");
    }
}
