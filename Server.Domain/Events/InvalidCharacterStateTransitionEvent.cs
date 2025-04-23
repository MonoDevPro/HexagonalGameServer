using System;
using CharacterState = Server.Domain.Enum.CharacterState;

namespace Server.Domain.Events;

/// <summary>
/// Evento disparado quando uma transição de estado inválida é tentada em um personagem.
/// </summary>
public class InvalidCharacterStateTransitionEvent : InvalidStateTransitionEvent<CharacterState>
{
    /// <summary>
    /// Cria uma nova instância do evento de transição de estado inválida para um personagem.
    /// </summary>
    /// <param name="currentState">O estado atual do personagem</param>
    /// <param name="attemptedState">O estado tentado</param>
    /// <param name="characterId">ID do personagem</param>
    /// <param name="errorMessage">Mensagem de erro explicando por que a transição é inválida</param>
    public InvalidCharacterStateTransitionEvent(
        CharacterState currentState, 
        CharacterState attemptedState, 
        long characterId, 
        string errorMessage) 
        : base(currentState, attemptedState, characterId, errorMessage)
    {
    }
}