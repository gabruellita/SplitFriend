using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<long>                          CreateAsync(CreateCategoryRequest request);
    Task                                UpdateAsync(long id, UpdateCategoryRequest request);
    Task                                DeactivateAsync(long id);
}
