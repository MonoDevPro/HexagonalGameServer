using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs.Primitives;

/// <summary>
/// Represents a rectangle with integer coordinates and dimensions.
/// Suitable for network serialization.
/// </summary>
public struct BoundingBoxDto : ISerializable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public void Serialize(INetworkWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteInt(Y);
        writer.WriteInt(Width);
        writer.WriteInt(Height);
    }

    public void Deserialize(INetworkReader reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadInt();
        Width = reader.ReadInt();
        Height = reader.ReadInt();
    }
    
    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
