using ChatService.Infrastructure.Security;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.API.Controllers;

[ApiController]
[Route("api/groups/{groupId:long}/messages")]
[Produces("application/json")]
public class MessagesController(IChatService service, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Istoric paginat (descrescator). `before` = id-ul celui mai vechi mesaj deja incarcat.</summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(long groupId, [FromQuery] long? before, [FromQuery] int limit = 50)
        => Ok(await service.GetHistoryAsync(groupId, currentUser.UserId, before, limit));
}
