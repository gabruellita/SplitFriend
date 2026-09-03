namespace FinanceService.Infrastructure.Models;

public class GroupExpense
{
    public long     Id           { get; set; }
    public long     GroupId      { get; set; }
    public long     PaidByUserId { get; set; }
    public string   Title        { get; set; } = string.Empty;
    public decimal  Amount       { get; set; }
    public long     CurrencyId   { get; set; }
    public string?  CurrencyCode { get; set; }
    public string   SplitType    { get; set; } = string.Empty;   // EQUAL / EXACT / PERCENT / SHARES
    public string   Status       { get; set; } = string.Empty;   // OPEN / SETTLED / CANCELED
    public DateOnly ExpenseDate  { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}
