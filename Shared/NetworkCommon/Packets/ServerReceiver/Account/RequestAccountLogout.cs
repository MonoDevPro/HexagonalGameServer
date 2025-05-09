using System;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Core.Domain.Models;

namespace NetworkCommon.Packets.ServerReceiver.Account;

/// <summary>
/// Classe marcadora para logout de conta
/// Não tem dados a serem enviados, apenas o tipo de pacote
/// </summary>
public class RequestAccountLogout : IPacket, ISerializable
{
    /// <summary>
    /// Serializa o pacote para envio pela rede
    /// Não há dados a serem serializados neste pacote
    /// </summary>
    /// <param name="writer">Escritor de rede</param>
    public void Serialize(INetworkWriter writer)
    {
        // Sem dados para serializar
    }
    
    /// <summary>
    /// Deserializa o pacote recebido da rede
    /// Não há dados a serem deserializados neste pacote
    /// </summary>
    /// <param name="reader">Leitor de rede</param>
    public void Deserialize(INetworkReader reader)
    {
        // Sem dados para deserializar
    }
    
    /// <summary>
    /// Retorna uma representação em string do pacote para depuração
    /// </summary>
    /// <returns>String representando o pacote</returns>
    public override string ToString()
    {
        return "RequestAccountLogout";
    }
}
