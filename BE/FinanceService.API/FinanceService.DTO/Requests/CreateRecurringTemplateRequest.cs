namespace FinanceService.DTO.Requests;

public record CreateRecurringTemplateRequest(
    decimal   Amount,
    string    Kind,            // "INCOME" / "EXPENSE"
    string    Frequency,       // "DAILY" / "WEEKLY" / "MONTHLY" / "YEARLY"
    int       IntervalCount,
    DateOnly  StartDate,
    DateOnly? EndDate,
    long?     CategoryId,
    long?     CurrencyId,
    string?   Description
);
