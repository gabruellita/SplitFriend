namespace ExportService.Infrastructure.Exceptions;

public class ValidationException(string message) : Exception(message);
