namespace Server.Domain.ValueObjects;

public readonly struct StatsVO : IEquatable<StatsVO>
{
    // Propriedades principais
    public double Strength { get; }
    public double Defense { get; }
    public double Agility { get; }

    // Propriedades derivadas
    public double AttackPower => Strength * 2.5;
    public double DamageReduction => Defense * 0.5;
    public double MovementSpeed => 1.0 + (Agility * 0.03);
    public double CriticalChance => Agility * 0.5;

    public StatsVO(double strength, double defense, double agility)
    {
        Strength = strength < 0 ? 0 : strength;
        Defense = defense < 0 ? 0 : defense;
        Agility = agility < 0 ? 0 : agility;
    }

    // Escalonar estatísticas por um fator
    public StatsVO Scale(double factor)
    {
        return new StatsVO(
            Strength * factor,
            Defense * factor,
            Agility * factor
        );
    }

    // Adicionar outro conjunto de estatísticas
    public StatsVO Add(StatsVO other)
    {
        return new StatsVO(
            Strength + other.Strength,
            Defense + other.Defense,
            Agility + other.Agility
        );
    }

    // Igualdade por valor
    public bool Equals(StatsVO other)
    {
        return Strength.Equals(other.Strength) &&
               Defense.Equals(other.Defense) &&
               Agility.Equals(other.Agility);
    }

    public override bool Equals(object? obj)
    {
        return obj is StatsVO other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Strength, Defense, Agility);
    }

    // Operadores
    public static bool operator ==(StatsVO left, StatsVO right) => left.Equals(right);
    public static bool operator !=(StatsVO left, StatsVO right) => !left.Equals(right);
    public static StatsVO operator +(StatsVO a, StatsVO b) => a.Add(b);
    public static StatsVO operator *(StatsVO stats, double factor) => stats.Scale(factor);

    public override string ToString() => $"STR:{Strength:F1} DEF:{Defense:F1} AGI:{Agility:F1}";

    // Conversão implícita da classe Stats para StatsVO
    public static implicit operator StatsVO(Entities.Primitives.Stats stats)
    {
        return new StatsVO(stats.Strength, stats.Defense, stats.Agility);
    }
}