using ChatService.DTO;
using ChatService.Infrastructure.Exceptions;
using ChatService.Infrastructure.Models;
using ChatService.Infrastructure.Repositories.Interfaces;
using ChatService.Services.Interfaces;

namespace ChatService.Services;

public class ChatAppService(
    IChatRepository            chatRepo,
    IGroupMembershipRepository membershipRepo
) : IChatService
{
    private const int MaxLimit = 100;

    public async Task<IEnumerable<MessageResponse>> GetHistoryAsync(long groupId, long userId, long? beforeId, int limit)
    {
        await EnsureMemberAsync(groupId, userId);
        var capped = limit is <= 0 or > MaxLimit ? 50 : limit;
        var messages = await chatRepo.GetMessagesAsync(groupId, beforeId, capped);
        return messages.Select(Map);
    }

    public async Task<MessageResponse> SendAsync(long groupId, long userId, string content, long? replyToId)
    {
        await EnsureMemberAsync(groupId, userId);
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Mesajul nu poate fi gol.");
        if (content.Length > 4000)
            throw new ValidationException("Mesajul depaseste 4000 de caractere.");

        var saved = await chatRepo.InsertMessageAsync(groupId, userId, content.Trim(), replyToId);
        return Map(saved);
    }

    public async Task<MessageResponse> EditAsync(long messageId, long userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Mesajul nu poate fi gol.");

        var rows = await chatRepo.EditMessageAsync(messageId, userId, content.Trim());
        if (rows == 0)
            throw new ForbiddenException("Nu poti edita acest mesaj (nu esti autorul sau e sters).");

        var updated = await chatRepo.GetMessageByIdAsync(messageId)
            ?? throw new NotFoundException("Mesajul nu exista.");
        return Map(updated);
    }

    public async Task<MessageResponse> DeleteAsync(long messageId, long userId)
    {
        var rows = await chatRepo.DeleteMessageAsync(messageId, userId);
        if (rows == 0)
            throw new ForbiddenException("Nu poti sterge acest mesaj (nu esti autorul sau e deja sters).");

        var deleted = await chatRepo.GetMessageByIdAsync(messageId)
            ?? throw new NotFoundException("Mesajul nu exista.");
        return Map(deleted);
    }

    public async Task EnsureMemberAsync(long groupId, long userId)
    {
        if (!await membershipRepo.IsMemberAsync(groupId, userId))
            throw new ForbiddenException("Nu esti membru al acestui grup.");
    }

    public Task<IEnumerable<long>> GetMemberIdsAsync(long groupId) => chatRepo.GetMemberIdsAsync(groupId);

    private static MessageResponse Map(ChatMessage m)
        => new(m.Id, m.GroupId, m.SenderUserId,
               m.DeletedAt is null ? m.Content : "",   // ascunde continutul sters
               m.ReplyToMessageId, m.CreatedAt, m.EditedAt, m.DeletedAt is not null);
}
