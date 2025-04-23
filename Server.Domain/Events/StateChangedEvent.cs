using System;

namespace Server.Domain.Events;

/// <summary>
/// Evento de domínio genérico para mudanças de estado em qualquer entidade.
/// </summary>
/// <typeparam name="TEnum">O tipo enum que representa os possíveis estados</typeparam>
public abstract class StateChangedEvent<TEnum> : DomainEvent where TEnum : System.Enum
{
    /// <summary>
    /// O estado anterior
    /// </summary>
    public TEnum PreviousState { get; }
    
    /// <summary>
    /// O novo estado
    /// </summary>
    public TEnum NewState { get; }
    
    /// <summary>
    /// O ID da entidade que teve seu estado alterado
    /// </summary>
    public long EntityId { get; }
    
    /// <summary>
    /// O momento da transição
    /// </summary>
    public DateTime TransitionTime { get; }
    
    /// <summary>
    /// Uma mensagem opcional que fornece contexto adicional sobre a transição
    /// </summary>
    public string? Reason { get; }

    protected StateChangedEvent(TEnum previousState, TEnum newState, long entityId, string? reason = null)
    {
        PreviousState = previousState;
        NewState = newState;
        EntityId = entityId;
        TransitionTime = DateTime.UtcNow;
        Reason = reason;
    }
}