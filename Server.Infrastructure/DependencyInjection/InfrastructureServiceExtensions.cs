using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Infrastructure.DependencyInjection;
using Server.Application.Handlers;
using Server.Application.Ports.Inbound;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Persistence;
using Server.Application.Ports.Outbound.Security;
using Server.Domain.Services;
using Server.Infrastructure.Messaging;
using Server.Infrastructure.Out;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Security;

namespace Server.Infrastructure.DependencyInjection;

/// <summary>
/// Extensões para registro dos serviços da camada de infraestrutura
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Adiciona todos os serviços de infraestrutura ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os adaptadores de infraestrutura registrados</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRepositories(configuration)
            .AddSecurity()
            .AddMessaging()
            .AddNetworking()
            .AddNetworkAdapters();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os repositórios ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os repositórios registrados</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        // Para ambiente de produção, use repositórios com banco de dados
        // Para ambiente de desenvolvimento/testes, use repositórios em memória
        
        // Por padrão, usamos repositórios em memória
        services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
        services.AddSingleton<ICharacterRepository, InMemoryCharacterRepository>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os serviços de segurança ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços de segurança registrados</returns>
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os serviços de mensageria ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços de mensageria registrados</returns>
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IGameEventPublisher, GameEventPublisher>();
        services.AddSingleton<IPlayerEventPublisher, PlayerEventPublisher>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os adaptadores de rede ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os adaptadores de rede registrados</returns>
    public static IServiceCollection AddNetworkAdapters(this IServiceCollection services)
    {
        services.AddScoped<IPlayerCommandHandler, NetworkPlayerCommandAdapter>();
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de rede ao container de DI
    /// </summary>
    public static IServiceCollection AddServerNetworking(this IServiceCollection services, IConfiguration config)
    {
        var networkConfig = config.GetSection("Network");

        string connectionKey = "default";

        // Carregar configuração de rede a partir do arquivo de configuração
        if (networkConfig != null && networkConfig.Exists())
        {
            
            var configChildren = networkConfig.GetChildren();

            foreach (var child in configChildren)
            {
                if (child.Key == "ConnectionKey")
                {
                    if (child.Value == null)
                    {
                        throw new ArgumentNullException(nameof(child.Value), "ConnectionKey cannot be null");
                    }
                    connectionKey = child.Value;
                }
            }
        }

        services.AddSingleton<INetworkConfiguration>(sp => 
        {
            var config = new NetworkConfiguration();
            // Aqui você pode carregar a configuração de rede a partir de um arquivo ou variável de ambiente
            // Exemplo: config.UpdateIntervalMs = 15;
            config.DisconnectTimeoutMs = 5000; // Timeout padrão
            config.ConnectionKey = connectionKey; // Chave de conexão padrão
            config.UseUnsyncedEvents = false; // Padrão: eventos processados apenas via Update()
            // Adicione outras configurações conforme necessário
            return config;
        });

        // Adiciona serviços do NetworkHexagonal usando a extensão existente
        services.AddNetworking();
        
        return services;
    }
}