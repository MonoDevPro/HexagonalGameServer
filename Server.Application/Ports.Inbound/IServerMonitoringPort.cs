namespace Server.Application.Ports.Inbound;

public interface IServerMonitoringPort
{
    /// <summary>
    /// Obtém o status atual do servidor
    /// </summary>
    /// <returns>Retorna informações sobre o estado atual do servidor</returns>
    Task<ServerStatusDto> GetServerStatusAsync();
    
    /// <summary>
    /// Obtém informações sobre os jogadores conectados
    /// </summary>
    /// <returns>Lista de jogadores online</returns>
    Task<IEnumerable<PlayerStatusDto>> GetOnlinePlayersAsync();
    
    /// <summary>
    /// Obtém métricas de performance do servidor
    /// </summary>
    /// <returns>Métricas de utilização de recursos</returns>
    Task<ServerMetricsDto> GetServerMetricsAsync();
}

public record ServerStatusDto(
    bool IsOnline,
    DateTime StartTime,
    TimeSpan Uptime,
    int ConnectedPlayers,
    string Version,
    string Environment);

public record PlayerStatusDto(
    string PlayerId,
    string Username,
    DateTime ConnectedSince,
    string CurrentZone);

public record ServerMetricsDto(
    double CpuUsage,
    long MemoryUsageMb,
    int ActiveThreads,
    int RequestsPerMinute,
    int ConnectionsPerMinute);
