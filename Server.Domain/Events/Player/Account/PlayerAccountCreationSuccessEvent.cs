namespace Server.Domain.Events.Player.Account;

/// <summary>
/// Evento disparado quando uma conta é criada com sucesso
/// </summary>
public class PlayerAccountCreationSuccessEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID da conta criada
    /// </summary>
    public long AccountId { get; }
    
    /// <summary>
    /// Nome de usuário da conta criada
    /// </summary>
    public string Username { get; }

    public PlayerAccountCreationSuccessEvent(int connectionId, long accountId, string username) 
        : base(connectionId, $"Account '{username}' created successfully")
    {
        AccountId = accountId;
        Username = username ?? throw new ArgumentNullException(nameof(username));
    }
}
