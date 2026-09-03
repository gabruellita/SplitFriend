namespace GateWay.DTO;

/// <summary>
/// Reprezentarea domeniului pentru o rută YARP.
/// Abstractizează sursa de configurare (appsettings.json, DB, service registry)
/// astfel încât layerul de servicii să nu depindă direct de IConfiguration.
/// </summary>
public class RouteConfigDto
{
    /// <summary>Identificatorul unic al rutei (ex: "identity-auth-public").</summary>
    public string  RouteId      { get; set; } = string.Empty;

    /// <summary>Cluster-ul downstream la care se trimite cererea (ex: "identity-cluster").</summary>
    public string  ClusterId    { get; set; } = string.Empty;

    /// <summary>Pattern-ul de cale pe care ruta îl interceptează (ex: "/api/identity/auth/{**catch-all}").</summary>
    public string  PathPattern  { get; set; } = string.Empty;

    /// <summary>True dacă ruta necesită un JWT valid — controlat prin AuthorizationPolicy YARP.</summary>
    public bool    RequiresAuth { get; set; }

    /// <summary>
    /// Prefixul eliminat din cale înainte de a trimite cererea downstream (ex: "/api/identity").
    /// Null înseamnă că nu se aplică PathRemovePrefix.
    /// </summary>
    public string? PathPrefix   { get; set; }
}
