namespace FinanceService.DTO.Responses;

public record RecurringTemplateResponse(
    long      Id,
    decimal   Amount,
    string    Kind,
    string    Frequency,
    int       IntervalCount,
    DateOnly  StartDate,
    DateOnly? EndDate,
    DateOnly  NextRunDate,
    bool      IsActive,
    long?     CategoryId,
    string?   CategoryName,
    long      CurrencyId,
    string?   CurrencyCode,
    string?   Description
);
