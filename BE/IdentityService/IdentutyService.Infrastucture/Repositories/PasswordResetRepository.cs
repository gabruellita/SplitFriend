using System.Data;
using Dapper;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class PasswordResetRepository(IDbConnectionFactory connectionFactory) : IPasswordResetRepository
{
    public async Task CreateAsync(long userId, string token, DateTime expiresAt)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteScalarAsync<long>(
            "sp_create_password_reset_token",
            new { p_user_id = userId, p_token = token, p_expires_at = expiresAt },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PasswordResetToken?> GetActiveAsync(string token)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<PasswordResetToken>(
            "sp_get_active_reset_token",
            new { p_token = token },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ConsumeAsync(string token)
    {
        using var conn = connectionFactory.CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "sp_consume_password_reset_token",
            new { p_token = token },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}
