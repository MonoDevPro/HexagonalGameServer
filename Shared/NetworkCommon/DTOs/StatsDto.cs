using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs;

/// <summary>
/// DTO para atributos de personagem
/// </summary>
public struct StatsDto : ISerializable
{
    /// <summary>
    /// Força do personagem
    /// </summary>
    public int Strength { get; set; }
    
    /// <summary>
    /// Defesa do personagem
    /// </summary>
    public int Defense { get; set; }
    
    /// <summary>
    /// Agilidade do personagem
    /// </summary>
    public int Agility { get; set; }
    
    /// <summary>
    /// Serializa o DTO para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteInt(Strength);
        writer.WriteInt(Defense);
        writer.WriteInt(Agility);
    }

    /// <summary>
    /// Deserializa o DTO recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Strength = reader.ReadInt();
        Defense = reader.ReadInt();
        Agility = reader.ReadInt();
    }
    
    /// <summary>
    /// Retorna uma representação em string do DTO para depuração
    /// </summary>
    /// <returns>String representando o DTO</returns>
    public override string ToString()
    {
        return $"StatsDto: Strength={Strength}, Defense={Defense}, Agility={Agility}";
    }
}