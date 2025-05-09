using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Classe base para eventos bem-sucedidos relacionados ao Player
/// </summary>
public abstract class PlayerSuccessEvent : PlayerEvent
{
    /// <summary>
    /// Mensagem descritiva sobre o sucesso da operação
    /// </summary>
    public string Message { get; }

    protected PlayerSuccessEvent(int connectionId, string message) : base(connectionId)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}