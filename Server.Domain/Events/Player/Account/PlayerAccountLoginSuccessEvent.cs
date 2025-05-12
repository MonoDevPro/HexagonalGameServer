namespace Server.Domain.Events.Player.Account;

/// <summary>
/// Evento disparado quando um login de conta é bem-sucedido
/// </summary>
public class PlayerAccountLoginSuccessEvent : PlayerSuccessEvent
{
    /// <summary>
    /// Nome de usuário da conta autenticada
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Momento do login
    /// </summary>
    public DateTime LoginAt { get; }

    public PlayerAccountLoginSuccessEvent(int connectionId, string username)
        : base(connectionId, $"Login successful for user {username}")
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        LoginAt = DateTime.UtcNow;
    }
}
