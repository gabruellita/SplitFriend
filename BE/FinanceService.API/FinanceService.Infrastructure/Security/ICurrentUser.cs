namespace FinanceService.Infrastructure.Security;

/// <summary>
/// Identitatea utilizatorului curent, extrasa din header-ele X-User-* injectate de Gateway.
/// Populata de CurrentUserMiddleware (in stratul API) pentru fiecare request.
/// </summary>
public interface ICurrentUser
{
    long  UserId     { get; }
    long? CurrencyId { get; }
}
