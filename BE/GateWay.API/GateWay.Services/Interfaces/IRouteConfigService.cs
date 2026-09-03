using GateWay.DTO;

namespace GateWay.Services.Interfaces;

/// <summary>
/// Serviciu de business logic pentru gestionarea configurației rutelor și clusterelor Gateway-ului.
/// Extinde repository-urile cu validare, logare și suport viitor pentru reload dinamic.
/// </summary>
public interface IRouteConfigService
{
    /// <summary>Returnează toate rutele configurate în Gateway.</summary>
    IReadOnlyList<RouteConfigDto> GetAllRoutes();

    /// <summary>Returnează toate clusterele (microserviciile downstream) înregistrate.</summary>
    IReadOnlyList<ClusterConfigDto> GetAllClusters();

    /// <summary>Caută o rută după ID. Null dacă nu există.</summary>
    RouteConfigDto? GetRouteById(string routeId);

    /// <summary>Caută un cluster după ID. Null dacă nu există.</summary>
    ClusterConfigDto? GetClusterById(string clusterId);

    /// <summary>Returnează rutele care necesită autentificare JWT.</summary>
    IReadOnlyList<RouteConfigDto> GetProtectedRoutes();

    /// <summary>Returnează rutele publice (fără autentificare).</summary>
    IReadOnlyList<RouteConfigDto> GetPublicRoutes();
}
