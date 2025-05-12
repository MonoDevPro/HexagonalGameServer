namespace Server.Domain.Events.Player.Character;

/// <summary>
/// Evento disparado quando um personagem do jogador sai do mundo do jogo
/// </summary>
public class PlayerCharacterLeftWorldEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID do personagem que saiu do mundo
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome do personagem que saiu do mundo
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Motivo da saída (desconexão, logout, troca de personagem, etc.)
    /// </summary>
    public string Reason { get; }

    public PlayerCharacterLeftWorldEvent(
        int connectionId, 
        long characterId, 
        string characterName,
        string reason) 
        : base(connectionId, $"Character '{characterName}' left world: {reason}")
    {
        CharacterId = characterId;
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}