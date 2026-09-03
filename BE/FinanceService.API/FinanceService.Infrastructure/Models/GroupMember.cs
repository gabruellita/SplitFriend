namespace FinanceService.Infrastructure.Models;

public class GroupMember
{
    public long      UserId    { get; set; }
    public string?   Email     { get; set; }
    public string?   Username  { get; set; }
    public string?   FirstName { get; set; }
    public string?   LastName  { get; set; }
    public string    Role      { get; set; } = string.Empty;   // OWNER / MEMBER
    public string    Status    { get; set; } = string.Empty;   // ACTIVE / INVITED / ...
    public DateTime? JoinedAt  { get; set; }
}
