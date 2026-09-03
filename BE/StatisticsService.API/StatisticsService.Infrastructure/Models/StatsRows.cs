namespace StatisticsService.Infrastructure.Models;

// 1. sp_stats_timeseries → bucket, kind, total
public class TimeseriesRow
{
    public DateOnly Bucket { get; set; }
    public string   Kind   { get; set; } = string.Empty;   // INCOME / EXPENSE
    public decimal  Total  { get; set; }
}

// 2. sp_stats_category_breakdown → category_id, category_name, total, cnt
public class CategoryBreakdownRow
{
    public long?    CategoryId   { get; set; }
    public string?  CategoryName { get; set; }
    public decimal  Total        { get; set; }
    public long     Cnt          { get; set; }
}

// 3. sp_stats_top_categories → category_name, total, pct
public class TopCategoryRow
{
    public string?  CategoryName { get; set; }
    public decimal  Total        { get; set; }
    public decimal? Pct          { get; set; }
}

// 4. sp_stats_calendar → zi, cnt, total
public class CalendarRow
{
    public DateOnly Zi    { get; set; }
    public long     Cnt   { get; set; }
    public decimal  Total { get; set; }
}

// 5. sp_stats_histogram → bucket, cnt
public class HistogramRow
{
    public int  Bucket { get; set; }
    public long Cnt    { get; set; }
}

// 6. sp_stats_savings_rate → luna, venituri, cheltuieli, rata
public class SavingsRateRow
{
    public DateOnly Luna       { get; set; }
    public decimal  Venituri   { get; set; }
    public decimal  Cheltuieli { get; set; }
    public decimal? Rata       { get; set; }
}

// 7. sp_stats_running_balance → zi, sold_cumulat
public class RunningBalanceRow
{
    public DateOnly Zi          { get; set; }
    public decimal  SoldCumulat { get; set; }
}

// 8. sp_stats_mom → perioada, total, total_anterior, variatie_pct
public class MoMRow
{
    public DateOnly Perioada      { get; set; }
    public decimal  Total         { get; set; }
    public decimal? TotalAnterior { get; set; }
    public decimal? VariatiePct   { get; set; }
}

// 9. sp_stats_pareto → category_name, total, pct_cumulat
public class ParetoRow
{
    public string?  CategoryName { get; set; }
    public decimal  Total        { get; set; }
    public decimal? PctCumulat   { get; set; }
}

// 10. sp_stats_weekday → dow, zi, total, cnt
public class WeekdayRow
{
    public int     Dow   { get; set; }
    public string  Zi    { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public long    Cnt   { get; set; }
}

// 11. sp_stats_recurring_split → este_recurenta, total, cnt
public class RecurringSplitRow
{
    public bool    EsteRecurenta { get; set; }
    public decimal Total         { get; set; }
    public long    Cnt           { get; set; }
}
