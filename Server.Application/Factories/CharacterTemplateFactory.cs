using System;
using Server.Domain.Entities.Primitives;
using Server.Domain.Enums;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Character;

namespace Server.Application.Factories;

/// <summary>
/// Factory para criar templates de personagens baseados em classes ou tipos pré-definidos
/// </summary>
public class CharacterTemplateFactory
{
    // Template básico com valores padrão
    public static CharacterCreationOptions CreateDefault(string name)
    {
        return new CharacterCreationOptions
        {
            Name = name,
            Stats = new Stats(strength: 10, defense: 10, agility: 10),
            Vitals = new Vitals(health: 100, maxHealth: 100, mana: 50, maxMana: 50),
            PositionBox = new PositionBox(0, 0, 32, 32),
            Direction = Direction.South,
            State = CharacterState.Idle,
            FloorIndex = 1
        };
    }

    // Templates específicos para cada classe
    // public static CharacterCreationOptions CreateWarrior(string name)
    // {
    //     return new CharacterCreationOptions
    //     {
    //         Name = name,
    //         Stats = new Stats { Strength = 15, Agility = 8, Defense = 12 },
    //         Vitals = new Vital { Health = 120, MaxHealth = 120, Mana = 30, MaxMana = 30 },
    //         PositionBox = new BoundingBox(0, 0, 32, 32),
    //         Direction = Direction.South,
    //         State = CharacterState.Idle,
    //         FloorIndex = 1,
    //         Class = CharacterClass.Warrior
    //     };
    // }

    // public static CharacterCreationOptions CreateMage(string name)
    // {
    //     return new CharacterCreationOptions
    //     {
    //         Name = name,
    //         Stats = new Stats { Strength = 6, Agility = 10, Defense = 6 },
    //         Vitals = new Vital { Health = 80, MaxHealth = 80, Mana = 100, MaxMana = 100 },
    //         PositionBox = new BoundingBox(0, 0, 32, 32),
    //         Direction = Direction.South,
    //         State = CharacterState.Idle,
    //         FloorIndex = 1,
    //         Class = CharacterClass.Mage
    //     };
    // }

    // public static CharacterCreationOptions CreateArcher(string name)
    // {
    //     return new CharacterCreationOptions
    //     {
    //         Name = name,
    //         Stats = new Stats { Strength = 8, Agility = 15, Defense = 8 },
    //         Vitals = new Vital { Health = 90, MaxHealth = 90, Mana = 60, MaxMana = 60 },
    //         PositionBox = new BoundingBox(0, 0, 32, 32),
    //         Direction = Direction.South,
    //         State = CharacterState.Idle,
    //         FloorIndex = 1,
    //         Class = CharacterClass.Archer
    //     };
    // }
    
    // // Método factory principal que escolhe o template baseado na classe
    // public static CharacterCreationOptions CreateForClass(string name, CharacterClass characterClass)
    // {
    //     return characterClass switch
    //     {
    //         CharacterClass.Warrior => CreateWarrior(name),
    //         CharacterClass.Mage => CreateMage(name),
    //         CharacterClass.Archer => CreateArcher(name),
    //         _ => CreateDefault(name) // Caso padrão
    //     };
    // }
}