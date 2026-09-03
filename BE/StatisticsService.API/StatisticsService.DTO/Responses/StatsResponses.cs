namespace StatisticsService.DTO.Responses;

// 1. Evolutie venituri vs cheltuieli (un punct per bucket + kind)
public record TimeseriesPointDto(DateOnly Bucket, string Kind, decimal Total);

// 2. Breakdown pe categorii
public record CategorySliceDto(long? CategoryId, string? CategoryName, decimal Total, long Count);

// 3. Top categorii + procent
public record TopCategoryDto(string? CategoryName, decimal Total, decimal? Pct);

// 4. Heatmap calendar
public record CalendarDayDto(DateOnly Day, long Count, decimal Total);

// 5. Histogram
public record HistogramBucketDto(int Bucket, long Count);

// 6. Rata de economisire
public record SavingsRateDto(DateOnly Month, decimal Income, decimal Expense, decimal? Rate);

// 7. Sold cumulativ
public record RunningBalanceDto(DateOnly Day, decimal Balance);

// 8. MoM / YoY
public record MoMPointDto(DateOnly Period, decimal Total, decimal? PreviousTotal, decimal? ChangePct);

// 9. Pareto
public record ParetoSliceDto(string? CategoryName, decimal Total, decimal? CumulativePct);

// 10. Pe ziua saptamanii
public record WeekdayDto(int Dow, string Day, decimal Total, long Count);

// 11. Recurente vs spontane
public record RecurringSplitDto(bool IsRecurring, decimal Total, long Count);
