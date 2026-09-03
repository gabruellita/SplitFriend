using GateWay.DTO;

namespace GateWay.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Contract pentru accesul la configurația clusterelor (servicii downstream).
/// Permite viitoare migrări către Consul, Kubernetes service discovery
/// fără a schimba codul din layerul de servicii.
/// </summary>
public interface IClusterConfigRepository
{
    /// <summary>Returnează toate clusterele (serviciile downstream) înregistrate.</summary>
    IReadOnlyList<ClusterConfigDto> GetAll();

    /// <summary>Caută un cluster după identificatorul său. Null dacă nu există.</summary>
    ClusterConfigDto? GetById(string clusterId);
}
