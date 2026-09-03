namespace FinanceService.DTO.Requests;

public record CreatePaymentRequest(
    long     ToUserId,
    decimal  Amount,
    string?  Method
);
