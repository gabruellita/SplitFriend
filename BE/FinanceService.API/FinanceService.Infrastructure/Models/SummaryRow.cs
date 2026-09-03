namespace FinanceService.Infrastructure.Models;

/// <summary>Rand agregat returnat de sp_get_summary (grupare pe kind + categorie).</summary>
public class SummaryRow
{
    public string  Kind             { get; set; } = string.Empty;   // INCOME / EXPENSE
    public long?   CategoryId       { get; set; }
    public string? CategoryName     { get; set; }
    public decimal TotalAmount      { get; set; }
    public long    TransactionCount { get; set; }
}
