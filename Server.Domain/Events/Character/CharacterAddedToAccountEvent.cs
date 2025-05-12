namespace Server.Domain.Events.Character;

public class CharacterAddedToAccountEvent : DomainEvent
{
    public long AccountId { get; }
    public long CharacterId { get; }
    
    public CharacterAddedToAccountEvent(
        long accountId, long characterId)
    {
        AccountId = accountId;
        CharacterId = characterId;
    }
}