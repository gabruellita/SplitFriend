namespace ChatService.Infrastructure.Repositories.Interfaces;

public interface IGroupMembershipRepository
{
    Task<bool> IsMemberAsync(long groupId, long userId);
}
