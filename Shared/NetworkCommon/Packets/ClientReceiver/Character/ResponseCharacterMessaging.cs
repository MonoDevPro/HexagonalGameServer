using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ClientReceiver.Character;

/// <summary>
/// Pacote de resposta para mensagens de chat do personagem
/// </summary>
public class ResponseCharacterMessaging : IPacket, ISerializable
{
    /// <summary>
    /// Nome do personagem que enviou a mensagem
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Conteúdo da mensagem
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(CharacterName);
        writer.WriteString(Message);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        CharacterName = reader.ReadString();
        Message = reader.ReadString();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseCharacterMessaging: CharacterName: {CharacterName}, Message: {Message}";
    }
}
