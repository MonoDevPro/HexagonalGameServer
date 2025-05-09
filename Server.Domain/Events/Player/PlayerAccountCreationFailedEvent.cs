using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando a criação de uma conta falha.
/// </summary>
public class PlayerAccountCreationFailedEvent : PlayerFailureEvent
{
    /// <summary>
    /// Nome de usuário que foi tentado criar
    /// </summary>
    public string Username { get; }

    public PlayerAccountCreationFailedEvent(int connectionId, string username, string reason) 
        : base(connectionId, "AccountCreation", "CreationFailed", reason)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
    }
}
