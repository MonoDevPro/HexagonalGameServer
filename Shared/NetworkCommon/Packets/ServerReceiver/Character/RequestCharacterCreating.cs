using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ServerReceiver.Character;

/// <summary>
/// Pacote de requisição para criação de personagem
/// </summary>
public class RequestCharacterCreating : IPacket, ISerializable
{
    /// <summary>
    /// Nome do personagem a ser criado
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(CharacterName);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        CharacterName = reader.ReadString();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"RequestCharacterCreating: {base.ToString()}, CharacterName: {CharacterName}";
    }
}
