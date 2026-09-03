namespace ExportService.Infrastructure.Exceptions;

public class UnauthorizedException(string message) : Exception(message);
