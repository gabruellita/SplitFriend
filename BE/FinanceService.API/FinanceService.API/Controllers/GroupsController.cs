using FinanceService.DTO.Requests;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/groups")]
[Produces("application/json")]
public class GroupsController(
    IGroupService                  service,
    IValidator<CreateGroupRequest> createValidator,
    IValidator<UpdateGroupRequest> updateValidator,
    IValidator<InviteMemberRequest> inviteValidator
) : ControllerBase
{
    /// <summary>Grupurile in care userul curent e membru activ.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    /// <summary>Un grup dupa id (doar daca esti membru).</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id) => Ok(await service.GetByIdAsync(id));

    /// <summary>Membrii unui grup.</summary>
    [HttpGet("{id:long}/members")]
    public async Task<IActionResult> GetMembers(long id) => Ok(await service.GetMembersAsync(id));

    /// <summary>Creeaza un grup (owner = userul curent).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var id = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Modifica numele/descrierea (doar owner).</summary>
    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateGroupRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Arhiveaza grupul (soft, doar owner).</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Archive(long id)
    {
        await service.ArchiveAsync(id);
        return NoContent();
    }

    /// <summary>Invita pe email (user existent → INVITED; necunoscut → pending invitation).</summary>
    [HttpPost("{id:long}/invite")]
    public async Task<IActionResult> Invite(long id, [FromBody] InviteMemberRequest request)
    {
        var validation = await inviteValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await service.InviteAsync(id, request));
    }

    /// <summary>Accepta invitatia in grupul curent.</summary>
    [HttpPost("{id:long}/accept")]
    public async Task<IActionResult> Accept(long id)
    {
        await service.AcceptAsync(id);
        return NoContent();
    }

    /// <summary>Paraseste grupul (blocat daca ai sold neachitat).</summary>
    [HttpPost("{id:long}/leave")]
    public async Task<IActionResult> Leave(long id)
    {
        await service.LeaveAsync(id);
        return NoContent();
    }
}
