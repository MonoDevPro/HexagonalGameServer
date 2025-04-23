
namespace Server.Domain.ValueObjects;

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
}