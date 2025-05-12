using Microsoft.Extensions.Options;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using Server.Application.Ports.Inbound;
using Server.Infrastructure.Inbound;

namespace Server.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Worker as a hosted service
        services.AddHostedService<Worker>();
        
        // Register configurations
        services.Configure<ServerConfig>(configuration.GetSection("Network"));
        services.Configure<WorkerOptions>(configuration.GetSection("Worker"));
        
        // Register the NetworkPlayerHandlerAdapter as a singleton
        services.AddSingleton<NetworkPlayerHandlerAdapter>(provider => 
        {
            var serverNetworkApp = provider.GetRequiredService<IServerNetworkApp>();
            var playerCommandHandler = provider.GetRequiredService<IPlayerCommandHandler>();
            
            return new NetworkPlayerHandlerAdapter(serverNetworkApp, playerCommandHandler);
        });
        
        return services;
    }
}

