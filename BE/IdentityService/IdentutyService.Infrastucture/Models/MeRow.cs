namespace IdentityService.Infrastructure.Models;

public class MeRow
{
    public long      Id                    { get; set; }
    public string    Email                 { get; set; } = "";
    public string    Username              { get; set; } = "";
    public string?   FirstName             { get; set; }
    public string?   LastName              { get; set; }
    public string    Status                { get; set; } = "";
    public long      PreferredCurrencyId   { get; set; }
    public string?   PreferredCurrencyCode { get; set; }
    public DateTime  CreatedAt             { get; set; }
}
