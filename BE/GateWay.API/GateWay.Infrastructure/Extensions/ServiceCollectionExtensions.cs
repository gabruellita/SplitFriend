using GateWay.Infrastructure.Configuration;
using GateWay.Infrastructure.Repositories;
using GateWay.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GateWay.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Înregistrează toate dependențele din layerul Infrastructure:
    /// - Opțiuni de configurare (JwtSettings, GatewaySettings)
    /// - Repository-uri pentru configurația rutelor și clusterelor YARP
    /// </summary>
    public static IServiceCollection AddGatewayInfrastructure(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // ── Binding opțiuni din appsettings ──────────────────────────────────────
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<GatewaySettings>(configuration.GetSection("Gateway"));

        // ── Repository-uri (Singleton — configurația nu se schimbă la runtime) ──
        services.AddSingleton<IRouteConfigRepository,   RouteConfigRepository>();
        services.AddSingleton<IClusterConfigRepository, ClusterConfigRepository>();

        return services;
    }
}
