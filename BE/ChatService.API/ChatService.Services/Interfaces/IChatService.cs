using ChatService.DTO;

namespace ChatService.Services.Interfaces;

public interface IChatService
{
    Task<IEnumerable<MessageResponse>> GetHistoryAsync(long groupId, long userId, long? beforeId, int limit);
    Task<MessageResponse> SendAsync(long groupId, long userId, string content, long? replyToId);
    Task<MessageResponse> EditAsync(long messageId, long userId, string content);
    Task<MessageResponse> DeleteAsync(long messageId, long userId);
    Task EnsureMemberAsync(long groupId, long userId);
    Task<IEnumerable<long>> GetMemberIdsAsync(long groupId);
}
