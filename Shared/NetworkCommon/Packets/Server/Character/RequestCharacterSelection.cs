using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Server.Character;

/// <summary>
/// Pacote de requisição para seleção de personagem
/// </summary>
public class RequestCharacterSelection : IPacket, ISerializable
{
    /// <summary>
    /// ID do personagem a ser selecionado
    /// </summary>
    public long CharacterId { get; set; }

    public void Serialize(INetworkWriter writer)
    {
        writer.WriteLong(CharacterId);
    }

    public void Deserialize(INetworkReader reader)
    {
        CharacterId = reader.ReadLong();
    }

    public override string ToString()
    {
        return $"RequestCharacterSelection: CharacterId: {CharacterId}";
    }
}
