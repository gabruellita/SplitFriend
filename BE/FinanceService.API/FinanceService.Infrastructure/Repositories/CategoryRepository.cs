using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class CategoryRepository(IDbConnectionFactory connectionFactory) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync(long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Category>(
            "sp_get_categories",
            new { p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Category?> GetByIdAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Category>(
            "sp_get_category_by_id",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateAsync(long userId, string name, string kind, string? icon, string? color)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_category",
            new { p_name = name, p_kind = kind, p_icon = icon, p_color = color, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(long id, long userId, string name, string? icon, string? color)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_update_category",
            new { p_id = id, p_user_id = userId, p_name = name, p_icon = icon, p_color = color },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> DeactivateAsync(long id, long userId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "sp_deactivate_category",
            new { p_id = id, p_user_id = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ValidForUserAsync(long id, long userId, string kind)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            "sp_category_valid_for_user",
            new { p_id = id, p_user_id = userId, p_kind = kind },
            commandType: CommandType.StoredProcedure);
    }
}
