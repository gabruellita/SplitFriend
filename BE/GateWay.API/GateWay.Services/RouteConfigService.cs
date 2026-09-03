using GateWay.DTO;
using GateWay.Infrastructure.Repositories.Interfaces;
using GateWay.Services.Interfaces;

namespace GateWay.Services;

/// <summary>
/// Implementare a <see cref="IRouteConfigService"/>.
/// Delegă accesul la date repository-urilor din layerul Infrastructure
/// și adaugă logică de filtrare specifică domeniului (rute protejate vs. publice).
///
/// Această separare permite:
/// - Unit testing al serviciului cu repository-uri mock
/// - Adăugarea de caching, logare sau validare fără a modifica repository-urile
/// - Extensie viitoare: reload dinamic al rutelor fără restart al aplicației
/// </summary>
public class RouteConfigService(
    IRouteConfigRepository   routeRepo,
    IClusterConfigRepository clusterRepo
) : IRouteConfigService
{
    public IReadOnlyList<RouteConfigDto>    GetAllRoutes()             => routeRepo.GetAll();
    public IReadOnlyList<ClusterConfigDto>  GetAllClusters()           => clusterRepo.GetAll();
    public RouteConfigDto?                  GetRouteById(string id)    => routeRepo.GetById(id);
    public ClusterConfigDto?                GetClusterById(string id)  => clusterRepo.GetById(id);

    public IReadOnlyList<RouteConfigDto> GetProtectedRoutes() =>
        routeRepo.GetAll()
                 .Where(r => r.RequiresAuth)
                 .ToList()
                 .AsReadOnly();

    public IReadOnlyList<RouteConfigDto> GetPublicRoutes() =>
        routeRepo.GetAll()
                 .Where(r => !r.RequiresAuth)
                 .ToList()
                 .AsReadOnly();
}
