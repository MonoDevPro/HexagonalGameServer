using System;
using CharacterState = Server.Domain.Enum.CharacterState;

namespace Server.Domain.Events;

/// <summary>
/// Evento disparado quando o estado de um personagem muda.
/// </summary>
public class CharacterStateChangedEvent : StateChangedEvent<CharacterState>
{
    /// <summary>
    /// Cria uma nova instância do evento de mudança de estado de personagem.
    /// </summary>
    /// <param name="previousState">Estado anterior do personagem</param>
    /// <param name="newState">Novo estado do personagem</param>
    /// <param name="characterId">ID do personagem</param>
    /// <param name="reason">Motivo opcional para a mudança de estado</param>
    public CharacterStateChangedEvent(
        CharacterState previousState, 
        CharacterState newState, 
        long characterId, 
        string? reason = null) 
        : base(previousState, newState, characterId, reason)
    {
    }
}