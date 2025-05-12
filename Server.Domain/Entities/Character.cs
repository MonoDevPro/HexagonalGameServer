using Server.Domain.Entities.Primitives;
using Server.Domain.Events.Character;
using Server.Domain.ValueObjects;
using Server.Domain.Enums;
using Server.Domain.ValueObjects.Account;
using Server.Domain.ValueObjects.Character;

namespace Server.Domain.Entities;

public class Character : Entity
{
    // Propriedades de domínio, com 'setters' protegidos para garantir invariantes
    public string Name { get; private set; }
    public Stats Stats { get; private set; }
    public Vitals Vital { get; private set; }
    public PositionBox BoundingBox { get; private set; }
    public Direction Direction { get; private set; }
    public int FloorIndex { get; private set; }
    
    // Construtor vazio para EF Core ou outros ORMs
    protected Character() { }

    // Construtor público para criação de domínio,
    // onde você valida invariantes e adiciona DomainEvents
    public Character(CharacterCreationOptions options)
    {
        options.ValidateAndThrow();
        
        // Atribuir os valores
        Name = options.Name;
        Stats = options.Stats;
        Vital = options.Vitals;
        BoundingBox = options.PositionBox;
        Direction = options.Direction;
        FloorIndex = options.FloorIndex;

        // Exemplo de DomainEvent levantado no Aggregate
        AddDomainEvent(new CharacterCreatedEvent(Id, Name));
    }

    // Métodos de comportamento do Character, que explicitam regras de negócio
}
