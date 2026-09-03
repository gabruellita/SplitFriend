namespace FinanceService.DTO.Requests;

public record CreateGroupExpenseRequest(
    string                              Title,
    decimal                             Amount,
    long                                PaidByUserId,
    string                              SplitType,      // EQUAL / EXACT / PERCENT / SHARES
    DateOnly                            ExpenseDate,
    IReadOnlyList<ExpenseParticipantInput> Participants
);

public record ExpenseParticipantInput(
    long     UserId,
    decimal? ExactAmount,   // pentru EXACT
    decimal? Percent,       // pentru PERCENT
    int?     Shares         // pentru SHARES
);
