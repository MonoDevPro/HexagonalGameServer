using System;
using CharacterState = Server.Domain.Enum.CharacterState;

namespace Server.Domain.Events;

/// <summary>
/// Evento disparado quando um personagem começa a se movimentar.
/// </summary>
public class CharacterStartedWalkingEvent : CharacterStateChangedEvent
{
    /// <summary>
    /// Direção do movimento
    /// </summary>
    public string Direction { get; }
    
    /// <summary>
    /// Posição inicial X
    /// </summary>
    public float StartPositionX { get; }
    
    /// <summary>
    /// Posição inicial Y
    /// </summary>
    public float StartPositionY { get; }
    
    /// <summary>
    /// Velocidade de movimento
    /// </summary>
    public float Speed { get; }

    /// <summary>
    /// Cria uma nova instância do evento de início de movimentação.
    /// </summary>
    /// <param name="previousState">Estado anterior do personagem</param>
    /// <param name="characterId">ID do personagem</param>
    /// <param name="direction">Direção do movimento</param>
    /// <param name="startPositionX">Posição X inicial</param>
    /// <param name="startPositionY">Posição Y inicial</param>
    /// <param name="speed">Velocidade de movimento</param>
    public CharacterStartedWalkingEvent(
        CharacterState previousState,
        long characterId,
        string direction,
        float startPositionX,
        float startPositionY,
        float speed) 
        : base(previousState, CharacterState.Walking, characterId, $"Começou a andar na direção {direction}")
    {
        Direction = direction;
        StartPositionX = startPositionX;
        StartPositionY = startPositionY;
        Speed = speed;
    }
}