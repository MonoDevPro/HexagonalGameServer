namespace Server.Domain.Events.Player.Connection;

/// <summary>
/// Evento emitido quando um jogador se desconecta do servidor
/// </summary>
public class PlayerDisconnectedEvent : PlayerEvent
{
    /// <summary>
    /// Motivo da desconexão
    /// </summary>
    public string Reason { get; }
    
    /// <summary>
    /// Momento da desconexão
    /// </summary>
    public DateTime DisconnectedAt { get; }

    public PlayerDisconnectedEvent(int connectionId, string reason) : base(connectionId)
    {
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        DisconnectedAt = DateTime.UtcNow;
    }
}