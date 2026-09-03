namespace ChatService.Services.Interfaces;

public interface IPresenceTracker
{
    /// <summary>Marcheaza userul online in grup. Intoarce true daca e prima conexiune a lui acolo.</summary>
    Task<bool> ConnectAsync(long groupId, long userId, string connectionId);
    /// <summary>Scoate conexiunea. Intoarce true daca userul nu mai are nicio conexiune in grup.</summary>
    Task<bool> DisconnectAsync(long groupId, long userId, string connectionId);
    Task<IEnumerable<long>> GetOnlineAsync(long groupId);
    Task<bool> IsOnlineAsync(long groupId, long userId);
}
