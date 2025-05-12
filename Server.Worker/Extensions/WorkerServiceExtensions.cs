using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using NetworkHexagonal.Core.Application.Services;
using NetworkHexagonal.Infrastructure.DependencyInjection;

namespace Server.Worker.Extensions;

/// <summary>
/// Extensões para registro dos serviços específicos do Worker
/// </summary>
public static class WorkerServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços de networking ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os serviços de networking registrados</returns>
    public static IServiceCollection AddServerNetworking(this IServiceCollection services, IConfiguration configuration)
    {
        // Adiciona serviços do NetworkHexagonal usando a extensão existente
        services.AddNetworking();
        
        // Obter porta do servidor da configuração
        int serverPort = configuration.GetValue<int>("Network:Port", 9050);
        
        // Registrar a configuração do servidor
        services.Configure<ServerConfig>(options =>
        {
            options.Port = serverPort;
        });
        
        // Registrar o serviço do servidor como singleton
        services.AddSingleton<IServerNetworkApp>(sp => 
        {
            var serverApp = sp.GetRequiredService<ServerApp>();
            serverApp.Initialize();
            return serverApp;
        });
        
        return services;
    }
}