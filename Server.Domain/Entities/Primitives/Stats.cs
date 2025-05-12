using Server.Domain.ValueObjects;

namespace Server.Domain.Entities.Primitives;

public class Stats : Entity, IEquatable<Stats>
{
    // Propriedades com validação básica para evitar valores negativos
    private double _strength;
    public double Strength 
    { 
        get => _strength; 
        set => _strength = value < 0 ? 0 : value; 
    }
    
    private double _defense;
    public double Defense 
    { 
        get => _defense; 
        set => _defense = value < 0 ? 0 : value; 
    }
    
    private double _agility;
    public double Agility 
    { 
        get => _agility; 
        set => _agility = value < 0 ? 0 : value; 
    }

    // Propriedades derivadas que poderiam ser usadas em cálculos do jogo
    public double AttackPower => Strength * 2.5;
    public double DamageReduction => Defense * 0.5;
    public double MovementSpeed => 1.0 + (Agility * 0.03);
    public double CriticalChance => Agility * 0.5;

    // Construtor padrão para ORM
    protected Stats() { }

    public  Stats(double strength, double defense, double agility)
    {
        Strength = strength;
        Defense = defense;
        Agility = agility;
    }

    // Métodos para manipulação de atributos
    public void ApplyBuff(StatsVO buff)
    {
        Strength += buff.Strength;
        Defense += buff.Defense;
        Agility += buff.Agility;
    }

    public Stats ScaleByLevel(int level, double multiplier = 0.1)
    {
        return new Stats(
            Strength * (1 + level * multiplier),
            Defense * (1 + level * multiplier),
            Agility * (1 + level * multiplier)
        );
    }

    // Verifica se os atributos estão dentro dos limites aceitáveis
    public bool IsValid(double minValue = 0, double maxValue = 100)
    {
        return Strength >= minValue && Strength <= maxValue &&
               Defense >= minValue && Defense <= maxValue &&
               Agility >= minValue && Agility <= maxValue;
    }
    
    // Clone this stats object
    public Stats Clone()
    {
        return new Stats(Strength, Defense, Agility);
    }

    public bool Equals(Stats? other)
    {
        if (other is null)
            return false;

        return Strength == other.Strength &&
               Defense == other.Defense &&
               Agility == other.Agility;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Stats);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Strength, Defense, Agility);
    }
    
    public override string ToString()
    {
        return $"STR:{Strength:F1} DEF:{Defense:F1} AGI:{Agility:F1}";
    }
}