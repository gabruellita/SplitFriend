namespace GateWay.DTO;

/// <summary>
/// Reprezentarea domeniului pentru un cluster YARP (serviciu downstream).
/// Un cluster poate conține mai multe destinații pentru load balancing —
/// în MVP avem o singură destinație "primary" per cluster.
/// </summary>
public class ClusterConfigDto
{
    /// <summary>Identificatorul unic al cluster-ului (ex: "identity-cluster").</summary>
    public string ClusterId          { get; set; } = string.Empty;

    /// <summary>Adresa destinației principale a serviciului (ex: "http://identity-api:8080/").</summary>
    public string DestinationAddress { get; set; } = string.Empty;
}
