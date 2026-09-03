namespace CurrencyService.Infrastructure.Exceptions;

/// <summary>Eroare de domeniu: monedă invalidă sau sursă de curs indisponibilă.</summary>
public class CurrencyException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
