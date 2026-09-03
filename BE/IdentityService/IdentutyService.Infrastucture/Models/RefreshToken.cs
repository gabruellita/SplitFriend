namespace IdentityService.Infrastructure.Models;

public class RefreshToken
{
    public long      Id        { get; set; }
    public long      UserId    { get; set; }
    public string    Token     { get; set; } = string.Empty;
    public long      FamilyId  { get; set; }
    public DateTime  ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime  CreatedAt { get; set; }

    // Proprietăți calculate — nu sunt coloane DB
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;
}
