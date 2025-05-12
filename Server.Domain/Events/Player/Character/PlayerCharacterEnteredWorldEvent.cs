namespace Server.Domain.Events.Player.Character;

/// <summary>
/// Evento disparado quando um personagem do jogador entra no mundo do jogo
/// </summary>
public class PlayerCharacterEnteredWorldEvent : PlayerSuccessEvent
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
    public int PositionX { get; }
    
    /// <summary>
    /// Posição Y do personagem
    /// </summary>
    public int PositionY { get; }

    public PlayerCharacterEnteredWorldEvent(
        int connectionId, 
        long characterId, 
        string characterName, 
        int floorIndex, 
        int positionX, 
        int positionY) 
        : base(connectionId, $"Character {characterName} Entered in World!")
    {
        CharacterId = characterId;
        CharacterName = characterName;
        FloorIndex = floorIndex;
        PositionX = positionX;
        PositionY = positionY;
    }
}