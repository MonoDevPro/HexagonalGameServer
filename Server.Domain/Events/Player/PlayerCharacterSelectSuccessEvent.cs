namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando a seleção de personagem é bem-sucedida
/// </summary>
public class PlayerCharacterSelectSuccessEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID do personagem selecionado
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome do personagem selecionado
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Nome da conta do jogador
    /// </summary>
    public string AccountName { get; }

    public PlayerCharacterSelectSuccessEvent(
        int connectionId,
        long characterId,
        string characterName,
        string accountName)
        : base(connectionId, $"Character '{characterName}' selected successfully")
    {
        CharacterId = characterId;
        CharacterName = characterName ?? throw new System.ArgumentNullException(nameof(characterName));
        AccountName = accountName ?? throw new System.ArgumentNullException(nameof(accountName));
    }
}