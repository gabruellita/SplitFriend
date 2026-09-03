using CurrencyService.API.Validators;
using CurrencyService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyService.API.Controllers;

[ApiController]
[Route("api/currency")]
[Produces("application/json")]
public class CurrencyController(
    IExchangeRateService rates,
    IValidator<ConvertQuery> convertValidator
) : ControllerBase
{
    /// <summary>Ratele curente față de o monedă de bază (default EUR).</summary>
    [HttpGet("rates")]
    public async Task<IActionResult> GetRates([FromQuery] string @base = "EUR", CancellationToken ct = default)
        => Ok(await rates.GetRatesAsync(@base, ct));

    /// <summary>Convertește o sumă dintr-o monedă în alta la cursul curent.</summary>
    [HttpGet("convert")]
    public async Task<IActionResult> Convert(
        [FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken ct = default)
    {
        var query = new ConvertQuery(from, to, amount);
        var validation = await convertValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await rates.ConvertAsync(from, to, amount, ct));
    }
}
