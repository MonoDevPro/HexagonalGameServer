using AccountState = Server.Domain.Enums.AccountState;

namespace Server.Domain.Events.Account;

/// <summary>
/// Evento disparado quando uma transição de estado inválida é tentada em uma conta.
/// </summary>
public class InvalidAccountStateTransitionEvent : InvalidStateTransitionEvent<AccountState>
{

    /// <summary>
    /// Cria uma nova instância do evento de transição de estado inválida para uma conta.
    /// </summary>
    /// <param name="currentState">O estado atual da conta</param>
    /// <param name="attemptedState">O estado tentado</param>
    /// <param name="accountId">ID da conta</param>
    /// <param name="errorMessage">Mensagem de erro explicando por que a transição é inválida</param>
    public InvalidAccountStateTransitionEvent(
        AccountState currentState, 
        AccountState attemptedState, 
        long accountId, 
        string username,
        string errorMessage) 
        : base(currentState, attemptedState, accountId, errorMessage)
    {
        AccountId = accountId;
        Username = username;
    }

    public long AccountId { get; }
    public string Username { get; }
    public string Reason => ErrorMessage;
}