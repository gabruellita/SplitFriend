using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionResponse>> GetAllAsync(DateOnly? from, DateOnly? to, long? categoryId, string? kind);
    Task<TransactionResponse>              GetByIdAsync(long id);
    Task<long>                             CreateAsync(CreateTransactionRequest request);
    Task                                   UpdateAsync(long id, UpdateTransactionRequest request);
    Task                                   VoidAsync(long id);
    Task<SummaryResponse>                  GetSummaryAsync(DateOnly? from, DateOnly? to);
}
