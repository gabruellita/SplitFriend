namespace FinanceService.Infrastructure.Models;

public class ExpenseSplit
{
    public long    UserId      { get; set; }
    public decimal OwedAmount  { get; set; }
    public decimal PaidAmount  { get; set; }
    public bool    IsSettled   { get; set; }
}
