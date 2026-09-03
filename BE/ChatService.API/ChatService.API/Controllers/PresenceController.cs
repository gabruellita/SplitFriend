using ChatService.Infrastructure.Security;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:long}/presence")]
[Produces("application/json")]
public class PresenceController(
    IChatService     chat,
    IPresenceTracker presence,
    ICurrentUser     currentUser
) : ControllerBase
{
    /// <summary>User-ii online acum in grup.</summary>
    [HttpGet]
    public async Task<IActionResult> GetOnline(long groupId)
    {
        await chat.EnsureMemberAsync(groupId, currentUser.UserId);
        return Ok(await presence.GetOnlineAsync(groupId));
    }
}
