using ChatService.Infrastructure.Models;

namespace ChatService.Infrastructure.Repositories.Interfaces;

public interface IChatRepository
{
    Task<ChatMessage> InsertMessageAsync(long groupId, long senderUserId, string content, long? replyToId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(long groupId, long? beforeId, int limit);
    Task<ChatMessage?> GetMessageByIdAsync(long id);
    Task<int> EditMessageAsync(long id, long senderUserId, string content);
    Task<int> DeleteMessageAsync(long id, long senderUserId);
    Task<IEnumerable<long>> GetMemberIdsAsync(long groupId);
}
