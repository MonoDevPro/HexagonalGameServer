using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Classe base para eventos de falha relacionados ao player
/// </summary>
public abstract class PlayerFailureEvent : PlayerEvent
{
    /// <summary>
    /// Operação que falhou
    /// </summary>
    public string Operation { get; }
    
    /// <summary>
    /// Código de erro ou tipo de falha
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Mensagem descritiva do erro
    /// </summary>
    public string ErrorMessage { get; }

    protected PlayerFailureEvent(
        int connectionId, 
        string operation, 
        string errorCode, 
        string errorMessage) 
        : base(connectionId)
    {
        Operation = operation;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}