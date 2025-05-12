using NetworkCommon.DTOs;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Client.Account;

/// <summary>
/// Pacote de resposta para login de conta
/// </summary>
public class ResponseAccountLogging : IPacket, ISerializable
{
    /// <summary>
    /// Dados da conta que fez login
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    public List<CharacterDto> Characters { get; set; } = [];
    
    /// <summary>
    /// Mensagem adicional (sucesso ou erro)
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(Username);
        
        writer.WriteInt(Characters.Count);
        foreach (var character in Characters)
        {
            writer.WriteSerializable(character);
        }

        writer.WriteString(Message);
    }
    
    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Username = reader.ReadString();
        
        var count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            Characters.Add(reader.ReadSerializable<CharacterDto>());
        }
        
        Message = reader.ReadString();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseAccountLogging: {Username}, Message: {Message}";
    }
}