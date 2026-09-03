using FinanceService.DTO.Requests;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/recurring-templates")]
[Produces("application/json")]
public class RecurringTemplatesController(
    IRecurringTemplateService                  service,
    IValidator<CreateRecurringTemplateRequest> createValidator,
    IValidator<UpdateRecurringTemplateRequest> updateValidator
) : ControllerBase
{
    /// <summary>Listeaza template-urile recurente ale userului.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok(await service.GetAllAsync());

    /// <summary>Returneaza un template recurent dupa id.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>Creeaza un template recurent (ex. chirie lunara).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRecurringTemplateRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var id = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Modifica un template recurent.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRecurringTemplateRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Dezactiveaza un template recurent (nu mai genereaza tranzactii).</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(long id)
    {
        await service.DeactivateAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Genereaza manual tranzactiile scadente din template-urile active ale userului
    /// (inlocuieste un scheduler/cron). Returneaza cate tranzactii s-au generat.
    /// </summary>
    [HttpPost("run-due")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunDue()
    {
        var generated = await service.RunDueAsync();
        return Ok(new FinanceService.DTO.Responses.RunDueResponse(generated));
    }
}
