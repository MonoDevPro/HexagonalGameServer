using NetworkCommon.DTOs.Enum;
using NetworkCommon.DTOs.Primitives;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ServerReceiver.Character;

/// <summary>
/// Pacote de requisição para uma ação de personagem
/// </summary>
public class RequestCharacterAction : IPacket, ISerializable
{
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
    /// Posição alvo do personagem
    /// </summary>
    public PointDto PositionDto { get; set; }
    
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteByte((byte)CurrentStateDto);
        writer.WriteByte((byte)NextStateDto);
        writer.WriteByte((byte)DirectionDto);
        writer.WriteSerializable(PositionDto);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
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
        return $"RequestCharacterAction: CurrentState: {CurrentStateDto}, NextState: {NextStateDto}, Direction: {DirectionDto}, TargetPosition: {PositionDto}";
    }
}