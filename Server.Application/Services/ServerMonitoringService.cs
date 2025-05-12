using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Server.Application.Ports.Inbound;

namespace Server.Application.Services;

public class ServerMonitoringService : IServerMonitoringPort
{
    private readonly ILogger<ServerMonitoringService> _logger;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private readonly string _version = "1.0.0"; // Você pode obter isto dinamicamente da assembly
    private readonly string _environment;

    public ServerMonitoringService(ILogger<ServerMonitoringService> logger, string environment = "Development")
    {
        _logger = logger;
        _environment = environment;
    }

    public Task<ServerStatusDto> GetServerStatusAsync()
    {
        _logger.LogInformation("Obtendo status do servidor");
        
        var status = new ServerStatusDto(
            IsOnline: true,
            StartTime: _startTime,
            Uptime: DateTime.UtcNow - _startTime,
            ConnectedPlayers: GetCurrentPlayerCount(),
            Version: _version,
            Environment: _environment
        );
        
        return Task.FromResult(status);
    }

    public Task<IEnumerable<PlayerStatusDto>> GetOnlinePlayersAsync()
    {
        _logger.LogInformation("Obtendo lista de jogadores online");
        
        // Esta seria uma implementação real que consultaria os jogadores online
        // Por enquanto, retornamos alguns dados de exemplo
        var players = new List<PlayerStatusDto>
        {
            new("player1", "Usuario1", DateTime.UtcNow.AddMinutes(-30), "Zona Inicial"),
            new("player2", "Usuario2", DateTime.UtcNow.AddMinutes(-15), "Cidade Principal")
        };
        
        return Task.FromResult(players.AsEnumerable());
    }

    public Task<ServerMetricsDto> GetServerMetricsAsync()
    {
        _logger.LogInformation("Obtendo métricas do servidor");

        // Numa implementação real, estas métricas viriam de monitoramento efetivo do sistema
        var process = Process.GetCurrentProcess();
        
        var metrics = new ServerMetricsDto(
            CpuUsage: GetCpuUsage(),
            MemoryUsageMb: process.WorkingSet64 / (1024 * 1024),
            ActiveThreads: process.Threads.Count,
            RequestsPerMinute: 120, // Exemplo
            ConnectionsPerMinute: 15  // Exemplo
        );
        
        return Task.FromResult(metrics);
    }
    
    // Métodos auxiliares para obter dados reais do servidor
    private int GetCurrentPlayerCount()
    {
        // Implementação que buscaria o número real de jogadores conectados
        return 25; // Valor de exemplo
    }
    
    private double GetCpuUsage()
    {
        // Implementação para obter uso real de CPU
        return 35.5; // Valor de exemplo (percentual)
    }
}
