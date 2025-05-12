using AccountState = Server.Domain.Enums.AccountState;

namespace Server.Domain.Events.Account;

/// <summary>
/// Evento disparado quando o estado de uma conta muda.
/// </summary>
public class AccountStateChangedEvent : StateChangedEvent<AccountState>
{
    /// <summary>
    /// Cria uma nova instância do evento de mudança de estado de conta.
    /// </summary>
    /// <param name="previousState">Estado anterior da conta</param>
    /// <param name="newState">Novo estado da conta</param>
    /// <param name="accountId">ID da conta</param>
    /// <param name="reason">Motivo opcional para a mudança de estado</param>
    public AccountStateChangedEvent(
        AccountState previousState, 
        AccountState newState, 
        long accountId, 
        string username,
        string? reason = null) 
        : base(previousState, newState, accountId, reason)
    {
        AccountId = accountId;
        Username = username;
    }

    public long AccountId { get; }
    public string Username { get; }
}