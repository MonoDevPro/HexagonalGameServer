using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando o login de uma conta falha
/// </summary>
public class PlayerAccountLoginFailedEvent : PlayerFailureEvent
{
    /// <summary>
    /// Nome de usuário que tentou fazer login
    /// </summary>
    public string Username { get; }

    public PlayerAccountLoginFailedEvent(int connectionId, string username, string reason) 
        : base(connectionId, "AccountLogin", "AuthenticationFailed", reason)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
    }
}
