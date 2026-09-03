namespace ExportService.DTO.Requests;

/// <summary>
/// Configurarea exportului PDF trimisa de FE.
/// mode = "MONTHS" → se foloseste Months (lista "YYYY-MM"); mode = "RANGE" → se foloseste Range.
/// Blocks: subset din { "SUMMARY", "TREND", "CATEGORIES", "TRANSACTIONS" }.
/// </summary>
public record ExportReportRequest(
    string                 Mode,
    IReadOnlyList<string>? Months,
    DateRange?             Range,
    IReadOnlyList<string>  Blocks,
    ExportOptions?         Options
);

public record DateRange(DateOnly From, DateOnly To);

public record ExportOptions(
    string? Granularity,                 // "DAILY" | "WEEKLY" | "MONTHLY" (default DAILY)
    bool    RunningBalanceInStatement,   // sold cumulativ pe rand in extras
    bool    CumulativeTotal              // total cumulat la final (relevant la MONTHS)
);
