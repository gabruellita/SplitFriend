namespace ExportService.Infrastructure.Exceptions;

/// <summary>Un serviciu downstream (Statistics/Finance) e oprit sau a dat timeout.</summary>
public class ServiceUnavailableException(string message) : Exception(message);
