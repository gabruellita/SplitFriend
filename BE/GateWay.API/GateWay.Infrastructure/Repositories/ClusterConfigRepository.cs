using GateWay.DTO;
using GateWay.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GateWay.Infrastructure.Repositories;

/// <summary>
/// Implementare a <see cref="IClusterConfigRepository"/> care citește clusterele YARP
/// din secțiunea "ReverseProxy:Clusters" din appsettings.json.
///
/// Fiecare cluster are o destinație "primary" cu adresa serviciului downstream.
/// În arhitecturi avansate, un cluster poate conține mai multe destinații
/// pentru load balancing — abstracția prin interfață permite această extensie.
/// </summary>
public class ClusterConfigRepository(IConfiguration configuration) : IClusterConfigRepository
{
    private const string ClustersSection    = "ReverseProxy:Clusters";
    private const string DestinationAddress = "Destinations:primary:Address";

    public IReadOnlyList<ClusterConfigDto> GetAll()
    {
        var section  = configuration.GetSection(ClustersSection);
        var clusters = new List<ClusterConfigDto>();

        foreach (var child in section.GetChildren())
            clusters.Add(MapToDto(child.Key, child));

        return clusters.AsReadOnly();
    }

    public ClusterConfigDto? GetById(string clusterId)
    {
        var section = configuration.GetSection($"{ClustersSection}:{clusterId}");
        return section.Exists() ? MapToDto(clusterId, section) : null;
    }

    private static ClusterConfigDto MapToDto(string clusterId, IConfigurationSection s) => new()
    {
        ClusterId          = clusterId,
        DestinationAddress = s[DestinationAddress] ?? string.Empty
    };
}
