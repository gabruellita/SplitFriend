using ChatService.Services.Interfaces;
using StackExchange.Redis;

namespace ChatService.Services;

public class UnreadService(IConnectionMultiplexer redis) : IUnreadService
{
    private IDatabase Db => redis.GetDatabase();
    private static string Key(long g, long u) => $"chat:unread:{g}:{u}";

    public async Task IncrementForAsync(long groupId, IEnumerable<long> userIds)
    {
        foreach (var u in userIds)
            await Db.StringIncrementAsync(Key(groupId, u));
    }

    public Task ResetAsync(long groupId, long userId) => Db.KeyDeleteAsync(Key(groupId, userId));

    public async Task<long> GetAsync(long groupId, long userId)
    {
        var v = await Db.StringGetAsync(Key(groupId, userId));
        return v.HasValue ? (long)v : 0;
    }

    public async Task<IReadOnlyDictionary<long, long>> GetAllForUserAsync(long userId, IEnumerable<long> groupIds)
    {
        var result = new Dictionary<long, long>();
        foreach (var g in groupIds)
            result[g] = await GetAsync(g, userId);
        return result;
    }
}
