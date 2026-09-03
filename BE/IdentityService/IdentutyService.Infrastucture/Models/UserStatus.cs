namespace IdentityService.Infrastructure.Models;

/// <summary>Constante pentru statusul utilizatorului — evită magic strings.</summary>
public static class UserStatus
{
    public const string Pending  = "PENDING";
    public const string Active   = "ACTIVE";
    public const string Inactive = "INACTIVE";
}
