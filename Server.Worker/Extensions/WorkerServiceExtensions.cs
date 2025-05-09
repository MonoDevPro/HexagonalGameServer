using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using NetworkHexagonal.Infrastructure.DependencyInjection;

namespace Server.Worker.Extensions;

/// <summary>
/// Extensões para registro dos serviços específicos do Worker
/// </summary>
public static class WorkerServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços do Worker ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os serviços do Worker registrados</returns>
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Adiciona o worker como serviço hospedado
        services.AddHostedService<Worker>();
        
        // Configuração específica do Worker
        services.Configure<WorkerOptions>(configuration.GetSection("Worker"));
        
        return services;
    }

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

/// <summary>
/// Opções de configuração para o Worker
/// </summary>
public class WorkerOptions
{
    /// <summary>
    /// Intervalo de atualização do Worker em milissegundos
    /// </summary>
    public int UpdateIntervalMs { get; set; } = 15;
}

/// <summary>
/// Configuração do servidor de rede
/// </summary>
public class ServerConfig
{
    /// <summary>
    /// Porta do servidor
    /// </summary>
    public int Port { get; set; }
}