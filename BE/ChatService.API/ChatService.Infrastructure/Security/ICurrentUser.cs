namespace ChatService.Infrastructure.Security;

public interface ICurrentUser
{
    long UserId { get; }
}
