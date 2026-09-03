namespace IdentityService.Infrastructure.Models;

public class PasswordResetToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
