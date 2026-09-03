using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;

namespace FinanceService.Services.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponse>> GetAllAsync(long groupId);
    Task<long> CreateAsync(long groupId, CreatePaymentRequest request);
}
