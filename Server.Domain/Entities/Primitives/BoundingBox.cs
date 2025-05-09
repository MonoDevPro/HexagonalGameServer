namespace Server.Domain.Entities.Primitives;

/// <summary>
/// Represents a rectangle with integer coordinates and dimensions.
/// Suitable for network serialization.
/// </summary>
public struct BoundingBox : IEquatable<BoundingBox>
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public BoundingBox(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static BoundingBox Empty => new(0, 0, 0, 0);

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

    public bool IntersectsWith(BoundingBox other)
    {
        return Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
    }

    public bool Equals(BoundingBox other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is BoundingBox other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(BoundingBox left, BoundingBox right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BoundingBox left, BoundingBox right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
