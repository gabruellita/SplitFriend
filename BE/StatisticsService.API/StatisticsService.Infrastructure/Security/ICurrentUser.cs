namespace StatisticsService.Infrastructure.Security;

/// <summary>Identitatea utilizatorului curent, extrasa din header-ele X-User-* injectate de Gateway.</summary>
public interface ICurrentUser
{
    long  UserId     { get; }
    long? CurrencyId { get; }
}
