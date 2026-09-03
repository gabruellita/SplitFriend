using System.Data;
using Dapper;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository(IDbConnectionFactory connectionFactory) : IRefreshTokenRepository
{
    public async Task<long> NextFamilyIdAsync()
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_next_token_family",
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreateAsync(long userId, string token, DateTime expiresAt, long familyId)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_create_refresh_token",
            new { p_user_id = userId, p_token = token, p_expires_at = expiresAt, p_family_id = familyId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<RefreshToken>(
            "sp_get_refresh_token_by_token",
            new { p_token = token },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeAsync(string token)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_revoke_refresh_token",
            new { p_token = token },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeFamilyAsync(long familyId)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_revoke_token_family",
            new { p_family_id = familyId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeAllForUserAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_revoke_all_refresh_tokens_for_user",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }
}
