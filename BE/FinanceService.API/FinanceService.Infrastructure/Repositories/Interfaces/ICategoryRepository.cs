using FinanceService.Infrastructure.Models;

namespace FinanceService.Infrastructure.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(long userId);
    Task<Category?>             GetByIdAsync(long id, long userId);
    Task<long>                  CreateAsync(long userId, string name, string kind, string? icon, string? color);
    Task<int>                   UpdateAsync(long id, long userId, string name, string? icon, string? color);
    Task<int>                   DeactivateAsync(long id, long userId);
    Task<bool>                  ValidForUserAsync(long id, long userId, string kind);
}
