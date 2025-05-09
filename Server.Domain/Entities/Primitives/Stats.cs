
namespace Server.Domain.Entities.Primitives;

public interface IStats
{
    double Strength { get; }
    double Defense { get; }
    double Agility { get; }
}

public struct Stats : IStats
{
    public double Strength { get; set; }
    public double Defense { get; set; }
    public double Agility { get; set; }

    public Stats(double strength, double defense, double agility)
    {
        Strength = strength;
        Defense = defense;
        Agility = agility;
    }
}