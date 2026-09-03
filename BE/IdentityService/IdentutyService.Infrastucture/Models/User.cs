namespace IdentityService.Infrastructure.Models;

public class User
{
    public long      Id                     { get; set; }
    public string    Email                  { get; set; } = string.Empty;
    public string    Username               { get; set; } = string.Empty;
    public string    PasswordHash           { get; set; } = string.Empty;
    public string?   FirstName              { get; set; }
    public string?   LastName               { get; set; }
    public string    Status                 { get; set; } = UserStatus.Pending;
    public long      PreferredCurrencyId    { get; set; }
    public string?   EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmedAt       { get; set; }
    public DateTime? LastLoginAt            { get; set; }
    public int       FailedLoginAttempts    { get; set; }
    public DateTime  CreatedAt              { get; set; }
    public DateTime  UpdatedAt              { get; set; }
}
