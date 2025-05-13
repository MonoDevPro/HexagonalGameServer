using Server.AdminMetrics.API.Application.Ports.Inbound;
using Server.AdminMetrics.API.Application.Ports.Inbound.Messaging;
using Server.AdminMetrics.API.Application.Ports.Outbound;
using Server.AdminMetrics.API.Application.Ports.Outbound.Messaging;
using Server.AdminMetrics.API.Application.Services.Handlers;
using Server.AdminMetrics.API.Application.Services.Messaging;
using Server.AdminMetrics.API.Infrastructure.Outbound;

namespace Server.AdminMetrics.API.Infrastructure.Configuration;

public static class ServerHttpServicesExtension
{
    /// <summary>
    /// Adiciona os serviços HTTP ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços HTTP registrados</returns>
    public static IServiceCollection AddServerHttpServices(this IServiceCollection services)
    {
        services.AddSingleton<IServerMonitoringCommandHandler, ServerMonitoringCommandHandler>();
        services.AddSingleton<IServerMonitoringPort, ServerMonitoring>();

        // Register HTTP event bus and subscriber
        services.AddSingleton<HttpSubscriber>();
        services.AddSingleton<IHttpEventBus, HttpEventBus>();
        services.AddSingleton<IHttpSubscriber>(sp => sp.GetRequiredService<HttpSubscriber>());

        return services;
    }
}
