using ChatService.DTO;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.API.Hubs;

public class ChatHub(IChatService chat, IPresenceTracker presence, IUnreadService unread) : Hub
{
    // Context.Items chei
    private const string GroupKey = "groupId";
    private const string UserKey  = "userId";

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext()!;
        var userOk  = long.TryParse(http.Request.Headers["X-User-Id"].FirstOrDefault(), out var userId);
        var groupOk = long.TryParse(http.Request.Query["groupId"].FirstOrDefault(), out var groupId);

        if (!userOk || !groupOk)
        {
            Context.Abort();
            return;
        }

        // Autorizare: doar membri activi intra in grupul SignalR.
        try { await chat.EnsureMemberAsync(groupId, userId); }
        catch { Context.Abort(); return; }

        Context.Items[GroupKey] = groupId;
        Context.Items[UserKey]  = userId;
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(groupId));

        var firstConnection = await presence.ConnectAsync(groupId, userId, Context.ConnectionId);
        await unread.ResetAsync(groupId, userId);   // intra in chat → necitite la 0
        if (firstConnection)
            await Clients.OthersInGroup(GroupName(groupId))
                .SendAsync("PresenceChanged", new ChatService.DTO.PresenceChangedDto(userId, true));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(GroupKey, out var g) && Context.Items.TryGetValue(UserKey, out var u))
        {
            var groupId = (long)g!; var userId = (long)u!;
            var wentOffline = await presence.DisconnectAsync(groupId, userId, Context.ConnectionId);
            if (wentOffline)
                await Clients.OthersInGroup(GroupName(groupId))
                    .SendAsync("PresenceChanged", new ChatService.DTO.PresenceChangedDto(userId, false));
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var (groupId, userId) = Current();
        var message = await chat.SendAsync(groupId, userId, dto.Content, dto.ReplyToMessageId);
        await Clients.Group(GroupName(groupId)).SendAsync("MessageReceived", message);

        var memberIds = await chat.GetMemberIdsAsync(groupId);
        var offline = new List<long>();
        foreach (var m in memberIds)
            if (m != userId && !await presence.IsOnlineAsync(groupId, m))
                offline.Add(m);
        await unread.IncrementForAsync(groupId, offline);
    }

    public async Task EditMessage(long messageId, string content)
    {
        var (groupId, userId) = Current();
        var message = await chat.EditAsync(messageId, userId, content);
        await Clients.Group(GroupName(groupId)).SendAsync("MessageEdited", message);
    }

    public async Task DeleteMessage(long messageId)
    {
        var (groupId, userId) = Current();
        var message = await chat.DeleteAsync(messageId, userId);
        await Clients.Group(GroupName(groupId)).SendAsync("MessageDeleted", message);
    }

    public async Task MarkRead()
    {
        var (groupId, userId) = Current();
        await unread.ResetAsync(groupId, userId);
    }

    private (long groupId, long userId) Current()
        => ((long)Context.Items[GroupKey]!, (long)Context.Items[UserKey]!);

    private static string GroupName(long groupId) => $"group:{groupId}";
}
