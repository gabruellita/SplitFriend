namespace FinanceService.Infrastructure.Exceptions;

public class ForbiddenException(string message) : Exception(message);
