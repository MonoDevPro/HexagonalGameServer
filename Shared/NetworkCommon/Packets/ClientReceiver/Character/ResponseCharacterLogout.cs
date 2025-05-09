using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ClientReceiver.Character;

/// <summary>
/// Pacote de resposta para logout de personagem
/// </summary>
public class ResponseCharacterLogout : IPacket, ISerializable
{
    /// <summary>
    /// ID do personagem que fez logout
    /// </summary>
    public long CharacterId { get; set; }
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteLong(CharacterId);
    }
    
    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        CharacterId = reader.ReadLong();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseCharacterLogout: CharacterId: {CharacterId}";
    }
}