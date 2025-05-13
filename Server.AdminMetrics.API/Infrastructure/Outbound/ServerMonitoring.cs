using System.Diagnostics;
using Server.AdminMetrics.API.Application.Events;
using Server.AdminMetrics.API.Application.Ports.Outbound;
using Server.AdminMetrics.API.Application.Ports.Outbound.Messaging;

namespace Server.AdminMetrics.API.Infrastructure.Outbound;

public class ServerMonitoring : IServerMonitoringPort
{
    private readonly ILogger<ServerMonitoring> _logger;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private readonly string _version = "1.0.0"; // Você pode obter isto dinamicamente da assembly
    private readonly string _environment;
    private readonly IHttpEventBus _eventPublisher;

    public ServerMonitoring(
        ILogger<ServerMonitoring> logger, 
        IHttpEventBus eventPublisher,
        string environment = "Development")
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _environment = environment;
    }

    public async Task GetServerStatusAsync()
    {
        _logger.LogInformation("Obtendo status do servidor");
        
        var statusEvent = new ServerStatusEvent(
            isOnline: true,
            startTime: _startTime,
            uptime: DateTime.UtcNow - _startTime,
            connectedPlayers: GetCurrentPlayerCount(),
            version: _version,
            environment: _environment
        );
        
        await _eventPublisher.PublishAsync(statusEvent);
    }

    public async Task GetOnlinePlayersAsync()
    {
        _logger.LogInformation("Obtendo lista de jogadores online");
        
        // Esta seria uma implementação real que consultaria os jogadores online
        // Por enquanto, publicamos eventos para cada jogador
        var players = new List<PlayerStatusEvent.ServerPlayerMonitoring>();
        
        // Gerar e publicar eventos para cada jogador
        var player1 = new PlayerStatusEvent.ServerPlayerMonitoring("player1", "Usuario1", DateTime.UtcNow.AddMinutes(-30), "Zona Inicial");
        var player2 = new PlayerStatusEvent.ServerPlayerMonitoring("player2", "Usuario2", DateTime.UtcNow.AddMinutes(-15), "Cidade Principal");
        players.Add(player1);
        players.Add(player2);
        var playerStatusEvent = new PlayerStatusEvent(players);
        
        await _eventPublisher.PublishAsync(playerStatusEvent);
    }

    public async Task GetServerMetricsAsync()
    {
        _logger.LogInformation("Obtendo métricas do servidor");

        // Numa implementação real, estas métricas viriam de monitoramento efetivo do sistema
        var process = Process.GetCurrentProcess();
        
        var metricsEvent = new ServerMetricsEvent(
            cpuUsage: GetCpuUsage(),
            memoryUsageMb: process.WorkingSet64 / (1024 * 1024),
            activeThreads: process.Threads.Count,
            requestsPerMinute: 120, // Exemplo
            connectionsPerMinute: 15  // Exemplo
        );
        
        await _eventPublisher.PublishAsync(metricsEvent);
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
