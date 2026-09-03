namespace FinanceService.Infrastructure.Models;

public class GroupBalanceRow
{
    public long    UserId       { get; set; }
    public string? Username     { get; set; }
    public long    CurrencyId   { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal NetAmount    { get; set; }   // + grupul ii datoreaza; − el datoreaza
}
