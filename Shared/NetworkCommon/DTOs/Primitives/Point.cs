using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs.Primitives;

/// <summary>
/// DTO para representar um ponto 2D com coordenadas inteiras
/// </summary>
public struct PointDto : ISerializable
{
    /// <summary>
    /// Coordenada X do ponto
    /// </summary>
    public int X { get; set; }
    
    /// <summary>
    /// Coordenada Y do ponto
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Serializa o DTO para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteInt(Y);
    }

    /// <summary>
    /// Deserializa o DTO recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        X = reader.ReadInt();
        Y = reader.ReadInt();
    }
    
    /// <summary>
    /// Retorna uma representação em string do DTO para depuração
    /// </summary>
    /// <returns>String representando o DTO</returns>
    public override string ToString()
    {
        return $"PointDto: ({X}, {Y})";
    }
}
