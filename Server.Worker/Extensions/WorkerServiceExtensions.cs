using NetworkHexagonal.Core.Application.Ports.Inbound;
using Server.AdminMetrics.API.Infrastructure.Inbound;
using Server.Application.Ports.Inbound;
using Server.Infrastructure.Inbound.Network;

namespace Server.Worker.Extensions;

/// <summary>
/// Extensões para registro dos serviços específicos do Worker
/// </summary>
public static class WorkerServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços específicos do Worker ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os serviços do Worker registrados</returns>
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
        
        // Adiciona serviços de Web API
        services.AddWebApi(configuration);
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de Web API para expor os endpoints HTTP
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os serviços de Web API registrados</returns>
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Adiciona os serviços do ASP.NET Core
        services.AddControllers()
            .AddApplicationPart(typeof(ServerMonitoringController).Assembly);
        
        // Adiciona o Swagger para documentação da API
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Game Server API", Version = "v1" });
        });
        
        return services;
    }
}

