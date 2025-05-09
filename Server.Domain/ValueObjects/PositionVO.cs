using Server.Domain.Enum;

namespace Server.Domain.ValueObjects;

/// <summary>
/// Represents an immutable position in a 2D grid with X and Y coordinates.
/// </summary>
public readonly record struct PositionVO
{
    /// <summary>
    /// Gets the X coordinate (horizontal position).
    /// </summary>
    public int X { get; init; }
    
    /// <summary>
    /// Gets the Y coordinate (vertical position).
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionVO"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate (horizontal position).</param>
    /// <param name="y">The Y coordinate (vertical position).</param>
    public PositionVO(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Returns a new position that is the result of moving from this position in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A new position after moving in the specified direction.</returns>
    public PositionVO Move(Direction direction) => direction switch
    {
        Direction.North => this with { Y = Y - 1 },
        Direction.East => this with { X = X + 1 },
        Direction.South => this with { Y = Y + 1 },
        Direction.West => this with { X = X - 1 },
        Direction.NorthEast => this with { X = X + 1, Y = Y - 1 },
        Direction.SouthEast => this with { X = X + 1, Y = Y + 1 },
        Direction.SouthWest => this with { X = X - 1, Y = Y + 1 },
        Direction.NorthWest => this with { X = X - 1, Y = Y - 1 },
        _ => this
    };

    // Since we're using a record struct, Equals and GetHashCode are auto-implemented
}