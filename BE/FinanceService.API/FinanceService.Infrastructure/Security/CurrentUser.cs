namespace FinanceService.Infrastructure.Security;

/// <summary>
/// Implementare scoped, populata o data per request de CurrentUserMiddleware.
/// </summary>
public class CurrentUser : ICurrentUser
{
    public long  UserId     { get; set; }
    public long? CurrencyId { get; set; }
}
