using Server.Domain.ValueObjects;

namespace Server.Domain.Entities.Primitives;

/// <summary>
/// Represents a rectangle with integer coordinates and dimensions.
/// Suitable for network serialization.
/// </summary>
public class PositionBox : Entity, IEquatable<PositionBox>
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public PositionBox(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static PositionBox Empty => new(0, 0, 0, 0);

    public Vector2 Location => new(X, Y);
    public Size Size => new(Width, Height);

    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public bool Contains(Vector2 point)
    {
        return point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
    }

    public bool IntersectsWith(PositionBox other)
    {
        return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    }

    public bool Equals(PositionBox? other)
    {
        if (other is null)
            return false;
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is PositionBox other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(PositionBox left, PositionBox right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PositionBox left, PositionBox right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
