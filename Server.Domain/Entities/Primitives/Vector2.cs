
namespace Server.Domain.Entities.Primitives;

public struct Vector2(int x, int y) : IEquatable<Vector2>
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    
    public static Vector2 Zero => new Vector2(0, 0);
    public static Vector2 Empty;

    public bool Equals(Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }
    
    public int DistanceTo(Vector2 to)
    {
        return (int)Math.Sqrt((X - to.X) * (X - to.X) + (Y - to.Y) * (Y - to.Y));
    }
    
    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !(left == right);
    }
    
    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X + right.X, left.Y + right.Y);
    }
    
    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X - right.X, left.Y - right.Y);
    }
    
    public static Vector2 operator *(Vector2 left, int right)
    {
        return new Vector2(left.X * right, left.Y * right);
    }
    
    public static Vector2 operator /(Vector2 left, int right)
    {
        return new Vector2(left.X / right, left.Y / right);
    }
    
    public static Vector2 operator /(Vector2 left, Vector2 right)
    {
        return new Vector2(left.X / right.X, left.Y / right.Y);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
    
    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }
}