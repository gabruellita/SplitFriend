namespace GateWay.Infrastructure.Configuration;

/// <summary>
/// Setări generale ale Gateway-ului, citite din secțiunea "Gateway" din appsettings.json.
/// </summary>
public class GatewaySettings
{
    /// <summary>
    /// Originile permise pentru CORS (ex: ["http://localhost:5173"]).
    /// Centralizat la nivel de gateway — microserviciile NU mai au CORS propriu.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}
