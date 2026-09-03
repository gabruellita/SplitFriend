namespace FinanceService.Infrastructure.Models;

public class Group
{
    public long     Id           { get; set; }
    public string   Name         { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public long     CurrencyId   { get; set; }
    public string?  CurrencyCode { get; set; }
    public long     OwnerUserId  { get; set; }
    public string   Status       { get; set; } = string.Empty;   // ACTIVE / ARCHIVED
    public long     MemberCount  { get; set; }
    public string?  MyRole       { get; set; }                    // OWNER / MEMBER
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}
