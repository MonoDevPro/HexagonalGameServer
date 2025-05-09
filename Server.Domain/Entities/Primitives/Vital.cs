
namespace Server.Domain.Entities.Primitives;

public interface IVital
{
    double Health { get; }
    double MaxHealth { get; }
    double Mana { get; }
    double MaxMana { get; }
}

public struct Vital : IVital, IEquatable<Vital>
{
    public double Health { get; set; }
    public double MaxHealth { get; set; }
    public double Mana { get; set; }
    public double MaxMana { get; set; }

    public Vital(double health, double maxHealth, double mana, double maxMana)
    {
        Health = health;
        MaxHealth = maxHealth;
        Mana = mana;
        MaxMana = maxMana;
    }

    public bool Equals(Vital other)
    {
        return Health.Equals(other.Health) && MaxHealth.Equals(other.MaxHealth) && Mana.Equals(other.Mana) && MaxMana.Equals(other.MaxMana);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vital other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Health, MaxHealth, Mana, MaxMana);
    }
}