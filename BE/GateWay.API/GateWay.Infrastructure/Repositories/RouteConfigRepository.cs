using GateWay.DTO;
using GateWay.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GateWay.Infrastructure.Repositories;

/// <summary>
/// Implementare a <see cref="IRouteConfigRepository"/> care citește rutele YARP
/// din secțiunea "ReverseProxy:Routes" din appsettings.json.
///
/// Permite abstractizarea sursei de configurare — dacă în viitor rutele vor fi
/// stocate într-o bază de date sau într-un service registry, se va implementa
/// o nouă versiune a interfeței fără a modifica layerul de servicii.
/// </summary>
public class RouteConfigRepository(IConfiguration configuration) : IRouteConfigRepository
{
    private const string RoutesSection          = "ReverseProxy:Routes";
    private const string ClusterIdKey           = "ClusterId";
    private const string PathMatchKey           = "Match:Path";
    private const string AuthorizationPolicyKey = "AuthorizationPolicy";

    // YARP stochează transform-urile ca array: Transforms:0:PathRemovePrefix
    private const string PathRemovePrefixKey    = "Transforms:0:PathRemovePrefix";

    public IReadOnlyList<RouteConfigDto> GetAll()
    {
        var section = configuration.GetSection(RoutesSection);
        var routes  = new List<RouteConfigDto>();

        foreach (var child in section.GetChildren())
            routes.Add(MapToDto(child.Key, child));

        return routes.AsReadOnly();
    }

    public RouteConfigDto? GetById(string routeId)
    {
        var section = configuration.GetSection($"{RoutesSection}:{routeId}");
        return section.Exists() ? MapToDto(routeId, section) : null;
    }

    private static RouteConfigDto MapToDto(string routeId, IConfigurationSection s) => new()
    {
        RouteId      = routeId,
        ClusterId    = s[ClusterIdKey]           ?? string.Empty,
        PathPattern  = s[PathMatchKey]           ?? string.Empty,
        RequiresAuth = s[AuthorizationPolicyKey] is not null,
        PathPrefix   = s[PathRemovePrefixKey]
    };
}
