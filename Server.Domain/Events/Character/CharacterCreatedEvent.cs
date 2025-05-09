namespace Server.Domain.Events.Character;

public class CharacterCreatedEvent : DomainEvent
{
    public long CharacterId { get; }
    public string Name { get; }

    public CharacterCreatedEvent(long characterId, string name)
    {
        CharacterId = characterId;
        Name = name;
    }
}