namespace Server.AdminMetrics.API.Application.Ports.Inbound.Messaging;

public interface IHttpSubscriber
{
    /// <summary>
    /// Assina eventos de monitoramento do servidor
    /// </summary>
    /// <param name="handler">Manipulador do evento</param>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <returns>Task representando a operação assíncrona</returns>
    Task<IAsyncDisposable> SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}
