namespace Server.AdminMetrics.API.Application.Ports.Outbound;

public interface IServerMonitoringPort
{
    /// <summary>
    /// Obtém o status atual do servidor
    /// </summary>
    /// <returns>Retorna informações sobre o estado atual do servidor</returns>
    Task GetServerStatusAsync();
    
    /// <summary>
    /// Obtém informações sobre os jogadores conectados
    /// </summary>
    /// <returns>Lista de jogadores online</returns>
    Task GetOnlinePlayersAsync();
    
    /// <summary>
    /// Obtém métricas de performance do servidor
    /// </summary>
    /// <returns>Métricas de utilização de recursos</returns>
    Task GetServerMetricsAsync();
}