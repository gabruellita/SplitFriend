using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface IRecurringTemplateService
{
    Task<IEnumerable<RecurringTemplateResponse>> GetAllAsync();
    Task<RecurringTemplateResponse>              GetByIdAsync(long id);
    Task<long>                                   CreateAsync(CreateRecurringTemplateRequest request);
    Task                                         UpdateAsync(long id, UpdateRecurringTemplateRequest request);
    Task                                         DeactivateAsync(long id);
    Task<int>                                    RunDueAsync();
}
