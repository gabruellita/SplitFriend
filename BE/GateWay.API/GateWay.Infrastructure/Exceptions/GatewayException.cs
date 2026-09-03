namespace GateWay.Infrastructure.Exceptions;

/// <summary>Excepție generică la nivel de Gateway (eroare internă de proxy/configurare).</summary>
public class GatewayException(string message) : Exception(message);
