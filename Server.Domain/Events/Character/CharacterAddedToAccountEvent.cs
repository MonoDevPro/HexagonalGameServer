namespace Server.Domain.Events.Character;

public class CharacterAddedToAccountEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }
    public long CharacterId { get; }
    public string CharacterName { get; }

    public CharacterAddedToAccountEvent(long accountId, string username, long characterId, string characterName)
    {
        AccountId = accountId;
        Username = username;
        CharacterId = characterId;
        CharacterName = characterName;
    }
}