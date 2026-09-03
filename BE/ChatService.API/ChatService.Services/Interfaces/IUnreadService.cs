namespace ChatService.Services.Interfaces;

public interface IUnreadService
{
    Task IncrementForAsync(long groupId, IEnumerable<long> userIds);
    Task ResetAsync(long groupId, long userId);
    Task<IReadOnlyDictionary<long, long>> GetAllForUserAsync(long userId, IEnumerable<long> groupIds);
    Task<long> GetAsync(long groupId, long userId);
}
