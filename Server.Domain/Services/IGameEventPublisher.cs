using Server.Domain.Events;

namespace Server.Domain.Services;

public interface IGameEventPublisher
{
    /// <summary>
    /// Publica um evento de domínio para consumo por sistemas externos.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento, imutável e derivado de DomainEvent.</typeparam>
    /// <param name="domainEvent">Instância do evento ocorrido no domínio.</param>
    Task PublishAsync<TEvent>(TEvent domainEvent)
        where TEvent : DomainEvent;
}