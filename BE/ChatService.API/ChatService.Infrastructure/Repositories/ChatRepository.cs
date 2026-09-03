using System.Data;
using Dapper;
using ChatService.Infrastructure.Models;
using ChatService.Infrastructure.Repositories.Interfaces;

namespace ChatService.Infrastructure.Repositories;

public class ChatRepository(IDbConnectionFactory connectionFactory) : IChatRepository
{
    public async Task<ChatMessage> InsertMessageAsync(long groupId, long senderUserId, string content, long? replyToId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleAsync<ChatMessage>(
            "sp_chat_insert_message",
            new { p_group_id = groupId, p_sender = senderUserId, p_content = content, p_reply_to = replyToId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(long groupId, long? beforeId, int limit)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<ChatMessage>(
            "sp_chat_get_messages",
            new { p_group_id = groupId, p_before_id = beforeId, p_limit = limit },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(long id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<ChatMessage>(
            "sp_chat_get_message_by_id",
            new { p_id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EditMessageAsync(long id, long senderUserId, string content)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_chat_edit_message",
            new { p_id = id, p_sender = senderUserId, p_content = content },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> DeleteMessageAsync(long id, long senderUserId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_chat_delete_message",
            new { p_id = id, p_sender = senderUserId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<long>> GetMemberIdsAsync(long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<long>(
            "sp_chat_get_member_ids",
            new { p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }
}
