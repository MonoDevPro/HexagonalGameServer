using NetworkCommon.DTOs;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Client.Character;

/// <summary>
/// Pacote de resposta para login de personagem
/// </summary>
public class ResponseCharacterLogging : IPacket, ISerializable
{
    /// <summary>
    /// ID do personagem logado
    /// </summary>
    public long CharacterId { get; set; }
    
    /// <summary>
    /// Dados do personagem logado
    /// </summary>
    public CharacterDto CharacterDto { get; set; }
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteLong(CharacterId);
        writer.WriteSerializable(CharacterDto);
    }
    
    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        CharacterId = reader.ReadLong();
        CharacterDto = reader.ReadSerializable<CharacterDto>();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseCharacterLogging: CharacterId: {CharacterId}, Character: {CharacterDto}";
    }
}