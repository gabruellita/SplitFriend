namespace FinanceService.DTO.Requests;

public record UpdateRecurringTemplateRequest(
    decimal   Amount,
    string    Kind,
    string    Frequency,
    int       IntervalCount,
    DateOnly? EndDate,
    long?     CategoryId,
    long?     CurrencyId,
    string?   Description
);
