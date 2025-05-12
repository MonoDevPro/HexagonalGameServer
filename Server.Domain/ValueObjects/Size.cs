namespace Server.Domain.ValueObjects;

public struct Size(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public static Size Zero => new(0, 0);

    public static Size Empty => new(0, 0);

    public static Size One => new(1, 1);

    public static Size operator +(Size a, Size b) => new(a.Width + b.Width, a.Height + b.Height);
}