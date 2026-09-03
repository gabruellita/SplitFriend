using GateWay.DTO;

namespace GateWay.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Contract pentru accesul la configurația rutelor YARP.
/// Abstractizează sursa de date (appsettings.json, DB, service registry)
/// — implementarea concretă poate fi înlocuită fără a modifica layerul de servicii.
/// </summary>
public interface IRouteConfigRepository
{
    /// <summary>Returnează toate rutele configurate în Gateway.</summary>
    IReadOnlyList<RouteConfigDto> GetAll();

    /// <summary>Caută o rută după identificatorul său unic. Null dacă nu există.</summary>
    RouteConfigDto? GetById(string routeId);
}
