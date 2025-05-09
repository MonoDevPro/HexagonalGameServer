using Microsoft.Extensions.DependencyInjection;
using Server.Application.Ports.Inbound;
using Server.Application.Services;

namespace Server.Application.DependencyInjection;

/// <summary>
/// Extensões para registro dos serviços da camada de aplicação
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços da camada de aplicação ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os serviços da aplicação registrados</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar serviços da camada de aplicação
        services.AddScoped<AccountService>();
        services.AddScoped<CharacterService>();
        services.AddScoped<PlayerService>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os handlers de comandos ao container de DI
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <returns>Collection de serviços com os handlers registrados</returns>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        // O registro de handlers pode ser feito aqui, se houver necessidade
        // Por exemplo, registrar implementações de IPlayerCommandHandler
        
        return services;
    }
}