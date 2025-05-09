namespace Server.Domain.Events.Character;

/// <summary>
/// Evento disparado quando a criação de um personagem falha.
/// </summary>
public class CharacterCreationFailedEvent : DomainEvent
{
    public string Username { get; }
    public string CharacterName { get; }
    public string Reason { get; }

    public CharacterCreationFailedEvent(string username, string characterName, string reason)
    {
        Username = username;
        CharacterName = characterName;
        Reason = reason;
    }
}
