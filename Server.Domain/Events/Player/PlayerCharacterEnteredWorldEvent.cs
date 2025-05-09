namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando um personagem do jogador entra no mundo do jogo
/// </summary>
public class PlayerCharacterEnteredWorldEvent : PlayerEvent
{
    /// <summary>
    /// ID do personagem que entrou no mundo
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome do personagem que entrou no mundo
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Índice do mapa/andar onde o personagem entrou
    /// </summary>
    public int FloorIndex { get; }
    
    /// <summary>
    /// Posição X do personagem
    /// </summary>
    public float PositionX { get; }
    
    /// <summary>
    /// Posição Y do personagem
    /// </summary>
    public float PositionY { get; }

    public PlayerCharacterEnteredWorldEvent(
        int connectionId, 
        long characterId, 
        string characterName, 
        int floorIndex, 
        float positionX, 
        float positionY) 
        : base(connectionId)
    {
        CharacterId = characterId;
        CharacterName = characterName;
        FloorIndex = floorIndex;
        PositionX = positionX;
        PositionY = positionY;
    }
}