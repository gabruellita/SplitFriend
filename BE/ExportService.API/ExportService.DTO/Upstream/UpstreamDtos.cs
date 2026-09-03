namespace ExportService.DTO.Upstream;

// ── Statistics :5004 ──
public record TimeseriesPoint(DateOnly Bucket, string Kind, decimal Total);
public record CategorySlice(long? CategoryId, string? CategoryName, decimal Total, long Count);
public record TopCategory(string? CategoryName, decimal Total, decimal? Pct);
public record SavingsRatePoint(DateOnly Month, decimal Income, decimal Expense, decimal? Rate);

// ── Finance :5002 ──
public record FinanceTransaction(
    long Id, decimal Amount, string Kind, DateOnly TransactionDate,
    long? CategoryId, string? CategoryName, long CurrencyId, string? CurrencyCode,
    string? Description, string Status, long? TemplateId, DateTime CreatedAt);

public record FinanceSummary(
    decimal TotalIncome, decimal TotalExpense, decimal Net,
    IReadOnlyList<FinanceSummaryCategory> ByCategory);

public record FinanceSummaryCategory(
    string Kind, long? CategoryId, string? CategoryName, decimal Total, long Count);
