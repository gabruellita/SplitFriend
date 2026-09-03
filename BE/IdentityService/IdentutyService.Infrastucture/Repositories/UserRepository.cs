using System.Data;
using Dapper;
using IdentityService.Infrastructure.Exceptions;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "sp_get_user_by_email",
            new { p_email = email },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "sp_get_user_by_id",
            new { p_id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_user_exists_by_email",
            new { p_email = email },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_user_exists_by_username",
            new { p_username = username },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateAsync(User user)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_user",
            new
            {
                p_email                    = user.Email,
                p_username                 = user.Username,
                p_password_hash            = user.PasswordHash,
                p_first_name               = user.FirstName,
                p_last_name                = user.LastName,
                p_status                   = user.Status,
                p_preferred_currency_id    = user.PreferredCurrencyId,
                p_email_confirmation_token = user.EmailConfirmationToken
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task ConfirmEmailAsync(string token)
    {
        using var conn = connectionFactory.CreateConnection();
        var rowsAffected = await conn.ExecuteScalarAsync<int>(
            "sp_confirm_email",
            new { p_token = token },
            commandType: CommandType.StoredProcedure);
        if (rowsAffected == 0)
            throw new ValidationException("Token invalid sau expirat.");
    }

    public async Task UpdateLastLoginAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_update_last_login",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task IncrementFailedAttemptsAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "sp_increment_failed_attempts",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> UpdateProfileAsync(long id, string? firstName, string? lastName, long? currencyId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "sp_update_user_profile",
            new { p_id = id, p_first_name = firstName, p_last_name = lastName, p_currency_id = currencyId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ChangePasswordAsync(long id, string newHash)
    {
        using var conn = connectionFactory.CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "sp_change_password",
            new { p_id = id, p_new_hash = newHash },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<MeRow?> GetProfileAsync(long id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<MeRow>(
            "sp_get_user_profile",
            new { p_id = id },
            commandType: CommandType.StoredProcedure);
    }
}
