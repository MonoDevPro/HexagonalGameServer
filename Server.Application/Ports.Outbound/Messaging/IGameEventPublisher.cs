using Server.Domain.Events;

namespace Server.Application.Ports.Outbound.Messaging;

public interface IGameEventPublisher
{
    /// <summary>
    /// Publica um evento de domínio para consumo por sistemas externos.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento, imutável e derivado de DomainEvent.</typeparam>
    /// <param name="domainEvent">Instância do evento ocorrido no domínio.</param>
    Task PublishAsync<TEvent>(TEvent domainEvent)
        where TEvent : DomainEvent;

    void Subscribe<TEvent>(Func<TEvent, Task> handler) 
        where TEvent : DomainEvent;

    void Subscribe<TEvent>(Action<TEvent> handler) 
        where TEvent : DomainEvent;

    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) 
        where TEvent : DomainEvent;

    void Unsubscribe<TEvent>(Action<TEvent> handler) 
        where TEvent : DomainEvent;
}