using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.DTOs;

/// <summary>
/// DTO para transferência de dados de conta pela rede
/// </summary>
public struct AccountDto : ISerializable
{
    /// <summary>
    /// Nome de usuário da conta
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Senha da conta (criptografada ou hashed)
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// Serializa o DTO para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(Username);
        writer.WriteString(Password);
    }

    /// <summary>
    /// Deserializa o DTO recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        Username = reader.ReadString();
        Password = reader.ReadString();
    }

    /// <summary>
    /// Retorna uma representação em string do DTO para depuração
    /// </summary>
    /// <returns>String representando o DTO</returns>
    public override string ToString()
    {
        return $"AccountDto: Username={Username}, Password={Password}";
    }
}