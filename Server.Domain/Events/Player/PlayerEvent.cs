using System;
using Server.Domain.Events;

namespace Server.Domain.Events.Player;

/// <summary>
/// Classe base para todos os eventos relacionados ao Player
/// </summary>
public abstract class PlayerEvent : DomainEvent
{
    /// <summary>
    /// ID de conexão do player (geralmente associado à sessão na camada de rede)
    /// </summary>
    public int ConnectionId { get; }
    
    /// <summary>
    /// Timestamp de quando o evento ocorreu
    /// </summary>
    public DateTime Timestamp { get; }

    protected PlayerEvent(int connectionId)
    {
        ConnectionId = connectionId;
        Timestamp = DateTime.UtcNow;
    }
}