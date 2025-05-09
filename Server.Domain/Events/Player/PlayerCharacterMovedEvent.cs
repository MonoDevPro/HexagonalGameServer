using System;
using Server.Domain.Enum;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento emitido quando um personagem de jogador se move no mundo
/// </summary>
public class PlayerCharacterMovedEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID do personagem que se moveu
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome do personagem
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Direção do movimento
    /// </summary>
    public Direction Direction { get; }
    
    /// <summary>
    /// Posição X do personagem após o movimento
    /// </summary>
    public float PositionX { get; }
    
    /// <summary>
    /// Posição Y do personagem após o movimento
    /// </summary>
    public float PositionY { get; }
    
    /// <summary>
    /// Índice do andar onde o personagem está
    /// </summary>
    public int FloorIndex { get; }
    
    /// <summary>
    /// Momento do movimento
    /// </summary>
    public DateTime MovementTimestamp { get; }

    public PlayerCharacterMovedEvent(
        int connectionId, 
        long characterId, 
        string characterName, 
        Direction direction, 
        float positionX, 
        float positionY, 
        int floorIndex)
        : base(connectionId, $"Character {characterName} moved {direction}")
    {
        CharacterId = characterId;
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        Direction = direction;
        PositionX = positionX;
        PositionY = positionY;
        FloorIndex = floorIndex;
        MovementTimestamp = DateTime.UtcNow;
    }
}