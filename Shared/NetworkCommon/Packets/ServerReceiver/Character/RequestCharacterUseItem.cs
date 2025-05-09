using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ServerReceiver.Character;

/// <summary>
/// Pacote de requisição para uso de item por um personagem
/// </summary>
public class RequestCharacterUseItem : IPacket, ISerializable
{
    /// <summary>
    /// ID do item a ser usado
    /// </summary>
    public int ItemId { get; set; }

    public void Serialize(INetworkWriter writer)
    {
        writer.WriteInt(ItemId);
    }

    public void Deserialize(INetworkReader reader)
    {
        ItemId = reader.ReadInt();
    }

    public override string ToString()
    {
        return $"RequestCharacterUseItem: ItemId: {ItemId}";
    }
}
