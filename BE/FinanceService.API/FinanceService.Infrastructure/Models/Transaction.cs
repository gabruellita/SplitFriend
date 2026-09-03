namespace FinanceService.Infrastructure.Models;

public class Transaction
{
    public long     Id              { get; set; }
    public long     UserId          { get; set; }
    public long?    CategoryId      { get; set; }
    public string?  CategoryName    { get; set; }
    public decimal  Amount          { get; set; }
    public long     CurrencyId      { get; set; }
    public string?  CurrencyCode    { get; set; }
    public string   Kind            { get; set; } = string.Empty;   // INCOME / EXPENSE
    public string?  Description     { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string   Status          { get; set; } = string.Empty;   // POSTED / VOIDED
    public long?    TemplateId      { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
}
