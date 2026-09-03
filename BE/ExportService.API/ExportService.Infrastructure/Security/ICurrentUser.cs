namespace ExportService.Infrastructure.Security;

public interface ICurrentUser
{
    long  UserId     { get; }
    long? CurrencyId { get; }
}
