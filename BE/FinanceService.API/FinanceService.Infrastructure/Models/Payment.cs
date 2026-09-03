namespace FinanceService.Infrastructure.Models;

public class Payment
{
    public long     Id                   { get; set; }
    public long     GroupId              { get; set; }
    public long     FromUserId           { get; set; }
    public long     ToUserId             { get; set; }
    public decimal  Amount               { get; set; }
    public long     CurrencyId           { get; set; }
    public string?  CurrencyCode         { get; set; }
    public decimal  OriginalAmount       { get; set; }
    public long     OriginalCurrencyId   { get; set; }
    public string?  OriginalCurrencyCode { get; set; }
    public decimal  ExchangeRate         { get; set; }
    public DateOnly RateDate             { get; set; }
    public string?  PaymentMethod        { get; set; }
    public DateTime PaidAt               { get; set; }
}
