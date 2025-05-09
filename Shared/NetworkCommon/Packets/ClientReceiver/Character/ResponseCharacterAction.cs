using NetworkCommon.DTOs.Enum;
using NetworkCommon.DTOs.Primitives;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ClientReceiver.Character;

/// <summary>
/// Pacote de resposta para ações do personagem
/// </summary>
public class ResponseCharacterAction : IPacket, ISerializable
{
    /// <summary>
    /// ID do personagem que realizou a ação
    /// </summary>
    public long CharacterId { get; set; }
    
    /// <summary>
    /// Estado atual do personagem
    /// </summary>
    public CharacterStateDto CurrentStateDto { get; set; }
    
    /// <summary>
    /// Próximo estado do personagem
    /// </summary>
    public CharacterStateDto NextStateDto { get; set; }
    
    /// <summary>
    /// Direção do personagem
    /// </summary>
    public DirectionDto DirectionDto { get; set; }
    
    /// <summary>
    /// Posição do personagem
    /// </summary>
    public PointDto PositionDto { get; set; }
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteLong(CharacterId);
        writer.WriteByte((byte)CurrentStateDto);
        writer.WriteByte((byte)NextStateDto);
        writer.WriteByte((byte)DirectionDto);
        PositionDto.Serialize(writer);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        CharacterId = reader.ReadLong();
        CurrentStateDto = (CharacterStateDto)reader.ReadByte();
        NextStateDto = (CharacterStateDto)reader.ReadByte();
        DirectionDto = (DirectionDto)reader.ReadByte();
        PositionDto = reader.ReadSerializable<PointDto>();
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseCharacterAction: CharacterId: {CharacterId}, CurrentState: {CurrentStateDto}, NextState: {NextStateDto}, Direction: {DirectionDto}, TargetPosition: {PositionDto}";
    }
}