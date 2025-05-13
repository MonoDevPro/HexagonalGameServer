namespace Server.AdminMetrics.API.Application.Ports.Outbound.Messaging;

public interface IHttpEventBus
{
    /// <summary>
    /// Publicar um evento de monitoramento do servidor
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="event"></param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}
