using FinanceService.DTO.Requests;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController(
    ICategoryService                  service,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator
) : ControllerBase
{
    /// <summary>Listeaza categoriile disponibile userului (system + custom).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok(await service.GetAllAsync());

    /// <summary>Creeaza o categorie custom pentru userul curent.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var id = await service.CreateAsync(request);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    /// <summary>Modifica o categorie custom proprie (categoriile system sunt read-only).</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCategoryRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Dezactiveaza (soft delete) o categorie custom proprie.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(long id)
    {
        await service.DeactivateAsync(id);
        return NoContent();
    }
}
