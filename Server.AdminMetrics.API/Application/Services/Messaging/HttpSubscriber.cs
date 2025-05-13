using System.Collections.Concurrent;
using Server.AdminMetrics.API.Application.Ports.Inbound.Messaging;

namespace Server.AdminMetrics.API.Application.Services.Messaging;

/// <summary>
/// Implementação do assinante de eventos HTTP
/// </summary>
public class HttpSubscriber : IHttpSubscriber
{
    private readonly ILogger<HttpSubscriber> _logger;
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();

    public HttpSubscriber(ILogger<HttpSubscriber> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Assina eventos de monitoramento do servidor
    /// </summary>
    /// <param name="handler">Manipulador do evento</param>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <returns>Task representando a operação assíncrona e um objeto descartável para cancelar a assinatura</returns>
    public Task<IAsyncDisposable> SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        _logger.LogInformation($"Assinando evento do tipo {eventType.Name}");

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<object>();
        }

        _handlers[eventType].Add(handler);
        
        // Retorna um objeto descartável que, quando descartado, cancela a assinatura
        var subscription = new Subscription<TEvent>(this, handler);
        return Task.FromResult<IAsyncDisposable>(subscription);
    }
    
    /// <summary>
    /// Publica um evento para todos os assinantes desse tipo de evento
    /// </summary>
    /// <param name="event">Evento a ser publicado</param>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <returns>Task representando a operação assíncrona</returns>
    internal async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug($"Nenhum assinante encontrado para o evento do tipo {eventType.Name}");
            return;
        }

        _logger.LogInformation($"Publicando evento do tipo {eventType.Name} para {handlers.Count} assinantes");
        
        foreach (var handler in handlers)
        {
            if (handler is Func<TEvent, Task> typedHandler)
            {
                try
                {
                    await typedHandler(@event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao processar evento {eventType.Name}: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Remove um manipulador de eventos específico
    /// </summary>
    /// <param name="handler">Manipulador a ser removido</param>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    internal void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            _logger.LogInformation($"Cancelada assinatura para evento do tipo {eventType.Name}");
            
            // Se não houver mais manipuladores para este tipo de evento, remova a entrada do dicionário
            if (handlers.Count == 0)
            {
                _handlers.TryRemove(eventType, out _);
            }
        }
    }

    /// <summary>
    /// Classe interna para gerenciar a assinatura e permitir o cancelamento
    /// </summary>
    private class Subscription<TEvent> : IAsyncDisposable where TEvent : class
    {
        private readonly HttpSubscriber _subscriber;
        private readonly Func<TEvent, Task> _handler;

        public Subscription(HttpSubscriber subscriber, Func<TEvent, Task> handler)
        {
            _subscriber = subscriber;
            _handler = handler;
        }

        public ValueTask DisposeAsync()
        {
            _subscriber.Unsubscribe(_handler);
            return ValueTask.CompletedTask;
        }
    }
}