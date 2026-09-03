using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class GroupRepository(IDbConnectionFactory connectionFactory) : IGroupRepository
{
    public async Task<long> CreateAsync(string name, string? description, long currencyId, long ownerId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_group",
            new { p_name = name, p_description = description, p_currency_id = currencyId, p_owner_id = ownerId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Group>> GetAllAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Group>(
            "sp_get_groups",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Group?> GetByIdAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Group>(
            "sp_get_group_by_id",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<string?> GetRoleAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<string?>(
            "sp_get_group_role",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> IsMemberAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_is_group_member",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(long id, long ownerId, string name, string? description)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_update_group",
            new { p_id = id, p_owner_id = ownerId, p_name = name, p_description = description },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ArchiveAsync(long id, long ownerId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_archive_group",
            new { p_id = id, p_owner_id = ownerId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<GroupMember>> GetMembersAsync(long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<GroupMember>(
            "sp_get_group_members",
            new { p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<long?> FindUserIdByEmailAsync(string email)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long?>(
            "sp_find_user_id_by_email",
            new { p_email = email },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<string?> GetMemberStatusAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<string?>(
            "sp_get_member_status",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InviteExistingUserAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_invite_existing_user",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreatePendingInvitationAsync(long groupId, string email, string token, DateTime expiresAt)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_pending_invitation",
            new { p_group_id = groupId, p_email = email, p_token = token, p_expires_at = expiresAt },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> AcceptInvitationAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_accept_invitation",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> LeaveGroupAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_leave_group",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<decimal> GetUserUnsettledAsync(long groupId, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>(
            "sp_get_user_unsettled",
            new { p_group_id = groupId, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }
}
