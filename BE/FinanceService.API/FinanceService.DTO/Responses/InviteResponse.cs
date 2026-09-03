namespace FinanceService.DTO.Responses;

/// <summary>Rezultatul invitatiei: INVITED_EXISTING (avea cont) sau PENDING_EMAIL (fara cont).</summary>
public record InviteResponse(string Outcome);
