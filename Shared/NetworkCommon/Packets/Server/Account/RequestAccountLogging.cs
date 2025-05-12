using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Server.Account;

/// <summary>
/// Pacote de requisição para login de conta
/// </summary>
public class RequestAccountLogging : IPacket, ISerializable
{
    /// <summary>
    /// Nome da conta
    /// </summary>
    public string AccountName { get; set; } = string.Empty;
    
    /// <summary>
    /// Senha da conta
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteString(AccountName);
        writer.WriteString(Password);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        AccountName = reader.ReadString();
        Password = reader.ReadString();
    }

    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"RequestAccountLogging: AccountName: {AccountName}, Password: {Password}";
    }
}
