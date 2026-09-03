namespace FinanceService.DTO.Responses;

public record GroupMemberResponse(
    long      UserId,
    string?   Email,
    string?   Username,
    string?   FirstName,
    string?   LastName,
    string    Role,
    string    Status,
    DateTime? JoinedAt
);
