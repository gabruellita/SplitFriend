using FinanceService.DTO.Requests;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:long}/expenses")]
[Produces("application/json")]
public class GroupExpensesController(
    IGroupExpenseService                  service,
    IValidator<CreateGroupExpenseRequest> createValidator
) : ControllerBase
{
    /// <summary>Cheltuielile grupului, cu split-urile lor.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(long groupId) => Ok(await service.GetAllAsync(groupId));

    /// <summary>O cheltuiala dupa id.</summary>
    [HttpGet("{expenseId:long}")]
    public async Task<IActionResult> GetById(long groupId, long expenseId)
        => Ok(await service.GetByIdAsync(groupId, expenseId));

    /// <summary>Adauga o cheltuiala de grup (calculeaza split-urile + tranzactii personale).</summary>
    [HttpPost]
    public async Task<IActionResult> Create(long groupId, [FromBody] CreateGroupExpenseRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var id = await service.CreateAsync(groupId, request);
        return CreatedAtAction(nameof(GetById), new { groupId, expenseId = id }, new { id });
    }

    /// <summary>Anuleaza o cheltuiala (CANCELED + VOID la tranzactiile personale legate).</summary>
    [HttpDelete("{expenseId:long}")]
    public async Task<IActionResult> Cancel(long groupId, long expenseId)
    {
        await service.CancelAsync(groupId, expenseId);
        return NoContent();
    }

    /// <summary>Soldurile nete per membru (settle-up overview).</summary>
    [HttpGet("/api/groups/{groupId:long}/balances")]
    public async Task<IActionResult> GetBalances(long groupId)
        => Ok(await service.GetBalancesAsync(groupId));
}
