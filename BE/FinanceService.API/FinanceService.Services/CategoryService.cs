using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;

namespace FinanceService.Services;

public class CategoryService(ICategoryRepository repo, ICurrentUser currentUser) : ICategoryService
{
    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await repo.GetAllAsync(currentUser.UserId);
        return categories.Select(MapToResponse);
    }

    public async Task<long> CreateAsync(CreateCategoryRequest request)
        => await repo.CreateAsync(currentUser.UserId, request.Name.Trim(), request.Kind, request.Icon, request.Color);

    public async Task UpdateAsync(long id, UpdateCategoryRequest request)
    {
        var rows = await repo.UpdateAsync(id, currentUser.UserId, request.Name.Trim(), request.Icon, request.Color);
        if (rows == 0)
            throw new NotFoundException("Categoria nu exista, nu va apartine sau este o categorie system (read-only).");
    }

    public async Task DeactivateAsync(long id)
    {
        var rows = await repo.DeactivateAsync(id, currentUser.UserId);
        if (rows == 0)
            throw new NotFoundException("Categoria nu exista, nu va apartine sau este o categorie system (read-only).");
    }

    private static CategoryResponse MapToResponse(Category c)
        => new(c.Id, c.Name, c.Kind, c.Icon, c.Color, c.IsSystem, c.IsActive);
}
