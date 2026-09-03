using IdentityService.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/currencies")]
[Produces("application/json")]
public class CurrenciesController(ICurrencyRepository repo) : ControllerBase
{
    /// <summary>Returneaza lista monedelor active (pentru dropdown la inregistrare).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var currencies = await repo.GetAllActiveAsync();
        return Ok(currencies);
    }
}
