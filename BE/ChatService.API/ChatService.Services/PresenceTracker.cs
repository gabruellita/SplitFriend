using ChatService.Services.Interfaces;
using StackExchange.Redis;

namespace ChatService.Services;

public class PresenceTracker(IConnectionMultiplexer redis) : IPresenceTracker
{
    private IDatabase Db => redis.GetDatabase();

    // Set de conexiuni per (grup,user): chat:conn:{groupId}:{userId} → {connectionId}
    private static string ConnKey(long g, long u) => $"chat:conn:{g}:{u}";
    // Set de useri online in grup: chat:online:{groupId} → {userId}
    private static string OnlineKey(long g) => $"chat:online:{g}";

    public async Task<bool> ConnectAsync(long groupId, long userId, string connectionId)
    {
        var added = await Db.SetAddAsync(ConnKey(groupId, userId), connectionId);
        var count = await Db.SetLengthAsync(ConnKey(groupId, userId));
        await Db.SetAddAsync(OnlineKey(groupId), userId);
        return count == 1 && added;   // prima conexiune a userului in grup
    }

    public async Task<bool> DisconnectAsync(long groupId, long userId, string connectionId)
    {
        await Db.SetRemoveAsync(ConnKey(groupId, userId), connectionId);
        var remaining = await Db.SetLengthAsync(ConnKey(groupId, userId));
        if (remaining == 0)
        {
            await Db.SetRemoveAsync(OnlineKey(groupId), userId);
            return true;   // userul a plecat complet din grup
        }
        return false;
    }

    public async Task<IEnumerable<long>> GetOnlineAsync(long groupId)
    {
        var members = await Db.SetMembersAsync(OnlineKey(groupId));
        return members.Select(v => (long)v);
    }

    public Task<bool> IsOnlineAsync(long groupId, long userId)
        => Db.SetContainsAsync(OnlineKey(groupId), userId);
}
