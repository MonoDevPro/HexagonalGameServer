using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs;

/// <summary>
/// DTO para valores vitais do personagem
/// </summary>
public struct VitalsDto : ISerializable
{
    /// <summary>
    /// Vida atual do personagem
    /// </summary>
    public int Health { get; set; }
    
    /// <summary>
    /// Vida máxima do personagem
    /// </summary>
    public int MaxHealth { get; set; }
    
    /// <summary>
    /// Mana atual do personagem
    /// </summary>
    public int Mana { get; set; }
    
    /// <summary>
    /// Mana máxima do personagem
    /// </summary>
    public int MaxMana { get; set; }
    
    /// <summary>
    /// Serializa o DTO para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteInt(Health);
        writer.WriteInt(MaxHealth);
        writer.WriteInt(Mana);
        writer.WriteInt(MaxMana);
    }

    /// <summary>
    /// Deserializa o DTO recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Health = reader.ReadInt();
        MaxHealth = reader.ReadInt();
        Mana = reader.ReadInt();
        MaxMana = reader.ReadInt();
    }
    
    /// <summary>
    /// Retorna uma representação em string do DTO para depuração
    /// </summary>
    /// <returns>String representando o DTO</returns>
    public override string ToString()
    {
        return $"VitalsDto: Health={Health}/{MaxHealth}, Mana={Mana}/{MaxMana}";
    }
}