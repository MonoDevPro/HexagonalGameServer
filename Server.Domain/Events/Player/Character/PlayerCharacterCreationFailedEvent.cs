namespace Server.Domain.Events.Player.Character;

/// <summary>
/// Evento disparado quando a criação de personagem falha
/// </summary>
public class PlayerCharacterCreationFailedEvent : PlayerFailureEvent
{
    /// <summary>
    /// Nome de usuário da conta
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Nome do personagem que tentou ser criado
    /// </summary>
    public string CharacterName { get; }

    public PlayerCharacterCreationFailedEvent(
        int connectionId,
        string username,
        string characterName,
        string reason)
        : base(connectionId, "CharacterCreation", "CharacterCreationFailed", reason)
    {
        Username = username; // Pode ser string vazia se o jogador não estiver autenticado
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
    }
}
