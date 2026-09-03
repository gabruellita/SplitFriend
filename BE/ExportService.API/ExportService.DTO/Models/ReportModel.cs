using ExportService.DTO.Upstream;

namespace ExportService.DTO.Models;

/// <summary>Modelul complet de raport, gata de randat in PDF.</summary>
public record ReportModel(
    ReportHeader            Header,
    IReadOnlyList<ReportSection> Sections,
    ReportSection?          CumulativeTotal   // null daca nu se cere / mode RANGE
);

public record ReportHeader(string UserLabel, string CurrencyCode, string PeriodLabel, DateTime GeneratedAt);

/// <summary>O sectiune = o luna sau intregul interval. Campurile sunt null daca blocul nu e cerut.</summary>
public record ReportSection(
    string                          Title,           // ex. "Martie 2026" sau "01.01.2026 – 30.06.2026"
    KpiBlock?                       Kpi,
    byte[]?                         TrendChartPng,
    byte[]?                         CategoryChartPng,
    IReadOnlyList<TopCategory>?     TopCategories,
    IReadOnlyList<FinanceTransaction>? Transactions,
    bool                            RunningBalanceInStatement
);

public record KpiBlock(decimal TotalIncome, decimal TotalExpense, decimal Net, decimal? SavingsRatePct);
