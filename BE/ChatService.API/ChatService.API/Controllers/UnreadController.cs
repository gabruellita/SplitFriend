using ChatService.Infrastructure.Security;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers;

[ApiController]
[Route("api/unread")]
[Produces("application/json")]
public class UnreadController(IUnreadService unread, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Necitite per grup pentru userul curent. `groups` = lista de id-uri (CSV), ex: ?groups=1,2,3</summary>
    [HttpGet]
    public async Task<IActionResult> GetUnread([FromQuery] string? groups)
    {
        var ids = (groups ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s, out var n) ? n : (long?)null)
            .Where(n => n is not null).Select(n => n!.Value)
            .ToList();
        return Ok(await unread.GetAllForUserAsync(currentUser.UserId, ids));
    }
}
