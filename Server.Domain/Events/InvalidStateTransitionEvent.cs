using System;

namespace Server.Domain.Events;

/// <summary>
/// Evento disparado quando uma transição de estado inválida é tentada.
/// </summary>
/// <typeparam name="TEnum">O tipo de enum de estado</typeparam>
public class InvalidStateTransitionEvent<TEnum> : DomainEvent where TEnum : System.Enum
{
    /// <summary>
    /// O estado atual da entidade
    /// </summary>
    public TEnum CurrentState { get; }
    
    /// <summary>
    /// O estado de destino que foi tentado, mas não é permitido
    /// </summary>
    public TEnum AttemptedState { get; }
    
    /// <summary>
    /// O ID da entidade
    /// </summary>
    public long EntityId { get; }
    
    /// <summary>
    /// Mensagem de erro que explica por que a transição é inválida
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Cria uma nova instância do evento de transição de estado inválida.
    /// </summary>
    /// <param name="currentState">O estado atual</param>
    /// <param name="attemptedState">O estado de destino tentado</param>
    /// <param name="entityId">ID da entidade</param>
    /// <param name="errorMessage">Mensagem de erro explicando por que a transição é inválida</param>
    public InvalidStateTransitionEvent(
        TEnum currentState, 
        TEnum attemptedState, 
        long entityId, 
        string errorMessage)
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
        EntityId = entityId;
        ErrorMessage = errorMessage;
    }
}