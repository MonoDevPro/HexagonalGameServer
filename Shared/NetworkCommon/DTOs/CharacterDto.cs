using NetworkCommon.DTOs.Enum;
using NetworkCommon.DTOs.Primitives;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs;

/// <summary>
/// DTO para transferência de dados de personagens pela rede
/// </summary>
public struct CharacterDto : ISerializable
{
    /// <summary>
    /// Nome do personagem
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Estatísticas do personagem
    /// </summary>
    public StatsDto StatsDto { get; set; }
    
    /// <summary>
    /// Atributos vitais do personagem
    /// </summary>
    public VitalsDto VitalsDto { get; set; }
    
    /// <summary>
    /// Caixa delimitadora para colisão
    /// </summary>
    public BoundingBoxDto BoundingBoxDto { get; set; }
    
    /// <summary>
    /// Direção que o personagem está olhando
    /// </summary>
    public DirectionDto DirectionDto { get; set; }
    
    /// <summary>
    /// Índice do andar/piso onde o personagem está
    /// </summary>
    public int FloorIndex { get; set; }

    /// <summary>
    /// Serializa o DTO para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(Name);
        writer.WriteSerializable(StatsDto);
        writer.WriteSerializable(VitalsDto);
        writer.WriteSerializable(BoundingBoxDto);
        writer.WriteByte((byte)DirectionDto);
        writer.WriteInt(FloorIndex);
    }

    /// <summary>
    /// Deserializa o DTO recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Name = reader.ReadString();
        StatsDto = reader.ReadSerializable<StatsDto>();
        VitalsDto = reader.ReadSerializable<VitalsDto>();
        BoundingBoxDto = reader.ReadSerializable<BoundingBoxDto>();
        DirectionDto = (DirectionDto)reader.ReadByte();
        FloorIndex = reader.ReadInt();
    }
}
