using System.Data;
using Dapper;
using ChatService.Infrastructure.Repositories.Interfaces;

namespace ChatService.Infrastructure.Repositories;

public class GroupMembershipRepository(IDbConnectionFactory connectionFactory) : IGroupMembershipRepository
{
    public async Task<bool> IsMemberAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_is_group_member",   // definit de planul Split Bill
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }
}
