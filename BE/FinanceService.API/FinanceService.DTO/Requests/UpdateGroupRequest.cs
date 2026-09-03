namespace FinanceService.DTO.Requests;

public record UpdateGroupRequest(
    string  Name,
    string? Description
);
