namespace Server.Domain.Events.Player.Character;

/// <summary>
/// Evento disparado quando a seleção de personagem falha
/// </summary>
public class PlayerCharacterSelectFailedEvent : PlayerFailureEvent
{
    /// <summary>
    /// ID do personagem que o jogador tentou selecionar
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome da conta do jogador
    /// </summary>
    public string AccountName { get; }

    public PlayerCharacterSelectFailedEvent(
        int connectionId,
        long characterId,
        string accountName,
        string reason)
        : base(connectionId, "CharacterSelect", "CharacterSelectFailed", reason)
    {
        CharacterId = characterId;
        AccountName = accountName ?? throw new System.ArgumentNullException(nameof(accountName));
    }
}