namespace Server.Domain.Events.Player.Connection;

/// <summary>
/// Evento emitido quando um novo jogador se conecta ao servidor
/// </summary>
public class PlayerConnectedEvent : PlayerEvent
{
    /// <summary>
    /// Momento da conexão
    /// </summary>
    public DateTime ConnectedAt { get; }
    
    public PlayerConnectedEvent(int connectionId) : base(connectionId)
    {
        ConnectedAt = DateTime.UtcNow;
    }
}