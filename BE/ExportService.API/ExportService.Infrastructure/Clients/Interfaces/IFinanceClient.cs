using ExportService.DTO.Upstream;

namespace ExportService.Infrastructure.Clients.Interfaces;

public interface IFinanceClient
{
    Task<IReadOnlyList<FinanceTransaction>> GetTransactionsAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<FinanceSummary>                    GetSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
