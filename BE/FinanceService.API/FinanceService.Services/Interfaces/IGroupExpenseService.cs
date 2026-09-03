using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface IGroupExpenseService
{
    Task<IEnumerable<GroupExpenseResponse>> GetAllAsync(long groupId);
    Task<GroupExpenseResponse> GetByIdAsync(long groupId, long expenseId);
    Task<long> CreateAsync(long groupId, CreateGroupExpenseRequest request);
    Task CancelAsync(long groupId, long expenseId);
    Task<IEnumerable<GroupBalanceResponse>> GetBalancesAsync(long groupId);
}
