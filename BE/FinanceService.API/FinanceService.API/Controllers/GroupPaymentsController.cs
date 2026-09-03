using FinanceService.DTO.Requests;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:long}/payments")]
[Produces("application/json")]
public class GroupPaymentsController(
    IPaymentService                  service,
    IValidator<CreatePaymentRequest> createValidator
) : ControllerBase
{
    /// <summary>Istoricul platilor de settle-up din grup.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(long groupId) => Ok(await service.GetAllAsync(groupId));

    /// <summary>Inregistreaza o plata (from = userul curent) si o aloca FIFO pe datorii.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(long groupId, [FromBody] CreatePaymentRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var id = await service.CreateAsync(groupId, request);
        return CreatedAtAction(nameof(GetAll), new { groupId }, new { id });
    }
}
