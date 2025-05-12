using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Server.Character;

/// <summary>
/// Pacote de requisição para logout de personagem
/// </summary>
public class RequestCharacterLogout : IPacket, ISerializable
{
    /// <summary>
    /// Motivo do logout (opcional)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(Reason);
    }
    
    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Reason = reader.ReadString();
    }

    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"RequestCharacterLogout: Reason: {Reason}";
    }
}