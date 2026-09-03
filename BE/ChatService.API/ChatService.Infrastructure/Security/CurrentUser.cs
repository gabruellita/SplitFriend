namespace ChatService.Infrastructure.Security;

public class CurrentUser : ICurrentUser
{
    public long UserId { get; set; }
}
