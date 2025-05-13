using Server.AdminMetrics.API.Application.Ports.Outbound.Messaging;

namespace Server.AdminMetrics.API.Application.Services.Messaging;

/// <summary>
/// Implementação do barramento de eventos HTTP que conecta publicadores e assinantes
/// </summary>
public class HttpEventBus : IHttpEventBus
{
    private readonly ILogger<HttpEventBus> _logger;
    private readonly HttpSubscriber _httpSubscriber;

    public HttpEventBus(
        ILogger<HttpEventBus> logger,
        HttpSubscriber httpSubscriber)
    {
        _logger = logger;
        _httpSubscriber = httpSubscriber;
    }

    /// <summary>
    /// Publica um evento para todos os assinantes registrados
    /// </summary>
    /// <param name="event">O evento a ser publicado</param>
    /// <typeparam name="TEvent">O tipo do evento</typeparam>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        _logger.LogInformation($"Publicando evento do tipo {typeof(TEvent).Name}");
        await _httpSubscriber.PublishAsync(@event);
    }
}