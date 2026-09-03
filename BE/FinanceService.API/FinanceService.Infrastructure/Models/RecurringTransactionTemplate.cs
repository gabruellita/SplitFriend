namespace FinanceService.Infrastructure.Models;

public class RecurringTransactionTemplate
{
    public long      Id            { get; set; }
    public long      UserId        { get; set; }
    public long?     CategoryId    { get; set; }
    public string?   CategoryName  { get; set; }
    public decimal   Amount        { get; set; }
    public long      CurrencyId    { get; set; }
    public string?   CurrencyCode  { get; set; }
    public string    Kind          { get; set; } = string.Empty;   // INCOME / EXPENSE
    public string?   Description   { get; set; }
    public string    Frequency     { get; set; } = string.Empty;   // DAILY / WEEKLY / MONTHLY / YEARLY
    public int       IntervalCount { get; set; }
    public DateOnly  StartDate     { get; set; }
    public DateOnly? EndDate       { get; set; }
    public DateOnly  NextRunDate   { get; set; }
    public bool      IsActive      { get; set; }
    public DateTime  CreatedAt     { get; set; }
    public DateTime  UpdatedAt     { get; set; }
}
