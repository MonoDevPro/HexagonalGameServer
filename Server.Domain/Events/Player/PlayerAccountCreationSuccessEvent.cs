using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando uma conta é criada com sucesso
/// </summary>
public class PlayerAccountCreationSuccessEvent : PlayerSuccessEvent
{
    /// <summary>
    /// Nome de usuário da conta criada
    /// </summary>
    public string Username { get; }

    public PlayerAccountCreationSuccessEvent(int connectionId, string username) 
        : base(connectionId, $"Account '{username}' created successfully")
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
    }
}
