using System.Globalization;
using ExportService.DTO.Models;
using ExportService.DTO.Requests;
using ExportService.DTO.Upstream;
using ExportService.Infrastructure.Charts;
using ExportService.Infrastructure.Clients.Interfaces;
using ExportService.Infrastructure.Pdf;
using ExportService.Services.Interfaces;

namespace ExportService.Services;

public class ReportService(
    IStatisticsClient stats,
    IFinanceClient    finance,
    IChartRenderer    charts,
    IPdfReportBuilder pdf) : IReportService
{
    private static readonly CultureInfo Ro = new("ro-RO");

    public async Task<(byte[] Pdf, string FileName)> GenerateAsync(
        ExportReportRequest request, string currencyCode, string userLabel, CancellationToken ct = default)
    {
        var granularity = MapGranularity(request.Options?.Granularity);
        var periods     = ResolvePeriods(request);
        var blocks      = request.Blocks.Select(b => b.ToUpperInvariant()).ToHashSet();
        var runningBal  = request.Options?.RunningBalanceInStatement ?? false;

        var sections = new List<ReportSection>();
        foreach (var (title, from, to) in periods)
            sections.Add(await BuildSectionAsync(title, from, to, blocks, granularity, runningBal, ct));

        ReportSection? cumulative = null;
        if (request.Mode.Equals("MONTHS", StringComparison.OrdinalIgnoreCase)
            && (request.Options?.CumulativeTotal ?? false)
            && periods.Count > 1)
        {
            var from = periods.Min(p => p.From);
            var to   = periods.Max(p => p.To);
            cumulative = await BuildSectionAsync("Total cumulat", from, to, blocks, granularity, runningBal, ct);
        }

        var periodLabel = $"{periods.Min(p => p.From).ToString("dd.MM.yyyy", Ro)} – {periods.Max(p => p.To).ToString("dd.MM.yyyy", Ro)}";
        var header = new ReportHeader(userLabel, currencyCode, periodLabel, DateTime.UtcNow);
        var model  = new ReportModel(header, sections, cumulative);

        var bytes    = pdf.Build(model);
        var fileName = $"raport-financiar-{periods.Min(p => p.From):yyyyMMdd}-{periods.Max(p => p.To):yyyyMMdd}.pdf";
        return (bytes, fileName);
    }

    private async Task<ReportSection> BuildSectionAsync(
        string title, DateOnly from, DateOnly to, HashSet<string> blocks,
        string granularity, bool runningBal, CancellationToken ct)
    {
        KpiBlock? kpi = null;
        byte[]?   trendPng = null;
        byte[]?   catPng   = null;
        IReadOnlyList<TopCategory>?     top  = null;
        IReadOnlyList<FinanceTransaction>? txns = null;

        if (blocks.Contains("SUMMARY"))
        {
            var summary = await finance.GetSummaryAsync(from, to, ct);
            var savings = await stats.GetSavingsRateAsync(from, to, ct);
            decimal? rate = savings.Count > 0
                ? savings.Where(s => s.Rate is not null).Select(s => s.Rate!.Value).DefaultIfEmpty().Average() * 100m
                : null;
            kpi = new KpiBlock(summary.TotalIncome, summary.TotalExpense, summary.Net, rate);
        }

        if (blocks.Contains("TREND"))
        {
            var ts = await stats.GetTimeseriesAsync(from, to, granularity, ct);
            trendPng = charts.RenderTrend(ts);
        }

        if (blocks.Contains("CATEGORIES"))
        {
            var slices = await stats.GetCategoryBreakdownAsync(from, to, "EXPENSE", ct);
            catPng = charts.RenderCategoryBreakdown(slices);
            top    = await stats.GetTopCategoriesAsync(from, to, "EXPENSE", 10, ct);
        }

        if (blocks.Contains("TRANSACTIONS"))
            txns = await finance.GetTransactionsAsync(from, to, ct);

        return new ReportSection(title, kpi, trendPng, catPng, top, txns, runningBal);
    }

    /// <summary>Returneaza lista de perioade (titlu, from, to) in functie de mode.</summary>
    private static List<(string Title, DateOnly From, DateOnly To)> ResolvePeriods(ExportReportRequest request)
    {
        if (request.Mode.Equals("RANGE", StringComparison.OrdinalIgnoreCase))
        {
            var r = request.Range!;
            var title = $"{r.From.ToString("dd.MM.yyyy", Ro)} – {r.To.ToString("dd.MM.yyyy", Ro)}";
            return [(title, r.From, r.To)];
        }

        // MONTHS: "YYYY-MM" → prima si ultima zi a lunii
        var result = new List<(string, DateOnly, DateOnly)>();
        foreach (var m in request.Months!.OrderBy(x => x))
        {
            var parts = m.Split('-');
            int year  = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int month = int.Parse(parts[1], CultureInfo.InvariantCulture);
            var first = new DateOnly(year, month, 1);
            var last  = first.AddMonths(1).AddDays(-1);
            var title = first.ToString("MMMM yyyy", Ro);
            title = char.ToUpper(title[0], Ro) + title[1..];
            result.Add((title, first, last));
        }
        return result.Select(x => (x.Item1, x.Item2, x.Item3)).ToList();
    }

    private static string MapGranularity(string? g) => (g?.ToUpperInvariant()) switch
    {
        "WEEKLY"  => "week",
        "MONTHLY" => "month",
        _         => "day",
    };
}
