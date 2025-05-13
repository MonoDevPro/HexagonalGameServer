using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Infrastructure.DependencyInjection;
using Server.Application.Handlers;
using Server.Application.Ports.Inbound;
using Server.Application.Ports.Outbound.Cache;
using Server.Application.Ports.Outbound.Messaging;
using Server.Application.Ports.Outbound.Networking;
using Server.Application.Ports.Outbound.Persistence;
using Server.Application.Ports.Outbound.Security;
using Server.Application.Ports.Outbound.Services;
using Server.Domain.Events.Player;
using Server.Infrastructure.Outbound.Messaging;
using Server.Infrastructure.Outbound.Networking;
using Server.Infrastructure.Outbound.Persistence.Memory;
using Server.Infrastructure.Outbound.Security;
using Server.Infrastructure.Outbound.Services;

namespace Server.Infrastructure.Configuration;

/// <summary>
/// Extensões para registro dos serviços da camada de infraestrutura
/// </summary>
public static class ServerServicesExtension
{
    /// <summary>
    /// Adiciona todos os serviços de infraestrutura ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os adaptadores de infraestrutura registrados</returns>
    public static IServiceCollection AddServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRepositories(configuration)
            .AddSecurity()
            .AddMessaging()
            .AddMapping() // Adicionado registro de serviços de mapeamento
            .AddServerNetworking(configuration)
            .AddCache(configuration);
        
        return services;
    }

    public static IServiceCollection AddServerApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register persistence services
        services.AddSingleton<IAccountServicePort, AccountService>();
        services.AddSingleton<ICharacterServicePort, CharacterService>();
        services.AddSingleton<IPlayerServicePort, PlayerService>();
        // Register command handlers
        services.AddSingleton<IPlayerCommandHandler, PlayerCommandHandler>();
        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPlayerCachePort, InMemoryPlayerCache>();
        return services;
    }
    
    /// <summary>
    /// Adiciona os repositórios ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Collection de serviços com os repositórios registrados</returns>
    private static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        // Para ambiente de produção, use repositórios com banco de dados
        // Para ambiente de desenvolvimento/testes, use repositórios em memória
        
        // Por padrão, usamos repositórios em memória
        services.AddSingleton<IAccountRepositoryPort, InMemoryAccountRepository>();
        services.AddSingleton<ICharacterRepositoryPort, InMemoryCharacterRepository>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os serviços de segurança ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços de segurança registrados</returns>
    private static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasherPort, PasswordHasher>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os serviços de mensageria ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços de mensageria registrados</returns>
    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisherPort, EventPublisher>();
        services.AddSingleton<IPlayerEventPublisherPort<PlayerEvent>, PlayerEventPublisher>();
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de mapeamento de DTOs ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços de mapeamento registrados</returns>
    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddSingleton<INetworkDtoMapperPort, NetworkDtoMapper>();
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de rede ao container de DI
    /// </summary>
    private static IServiceCollection AddServerNetworking(this IServiceCollection services, IConfiguration config)
    {
        var networkConfig = config.GetSection("Network");

        // Valores padrão
        string connectionKey = "HexagonalGameServer";
        int disconnectTimeoutMs = 5000;
        bool useUnsyncedEvents = false;

        // Carregar configuração de rede a partir do arquivo de configuração
        if (networkConfig.Exists())
        {
            connectionKey = networkConfig.GetSection("ConnectionKey").Value ?? connectionKey;
            var timeoutValue = 0;
            if (int.TryParse(networkConfig.GetSection("DisconnectTimeoutMs").Value, out timeoutValue))
            {
                disconnectTimeoutMs = timeoutValue;
            }
            bool.TryParse(networkConfig.GetSection("UseUnsyncedEvents").Value, out useUnsyncedEvents);
        }

        services.AddSingleton<INetworkConfiguration>(sp => 
        {
            var networkConfiguration = new NetworkConfiguration
            {
                DisconnectTimeoutMs = disconnectTimeoutMs,
                ConnectionKey = connectionKey,
                UseUnsyncedEvents = useUnsyncedEvents
                // Adicione outras configurações conforme necessário
            };
            return networkConfiguration;
        });

        // Adiciona serviços do NetworkHexagonal usando a extensão existente
        services.AddNetworking();
        
        return services;
    }
}

