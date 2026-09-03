using GateWay.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GateWay.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Înregistrează toate dependențele din layerul Services.
    /// </summary>
    public static IServiceCollection AddGatewayServices(this IServiceCollection services)
    {
        // Singleton — stateless, citesc din repository-uri care sunt tot Singleton
        services.AddSingleton<IRouteConfigService, RouteConfigService>();

        return services;
    }
}
