using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.Client.Account;

/// <summary>
/// Pacote de resposta para criação de conta
/// </summary>
public class ResponseAccountCreating : IPacket, ISerializable
{
    /// <summary>
    /// ID da conta criada
    /// </summary>
    public long AccountId { get; set; }
    
    /// <summary>
    /// Mensagem de resposta (sucesso ou erro)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        writer.WriteLong(AccountId);
        writer.WriteString(Message);
    }

    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        AccountId = reader.ReadLong();
        Message = reader.ReadString();
    }

    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return $"ResponseAccountCreating: AccountId: {AccountId}, Message: {Message}";
    }
}