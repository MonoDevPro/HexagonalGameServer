using Microsoft.Extensions.Options;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using NetworkHexagonal.Core.Domain.Events.Network;
using Server.Infrastructure.Inbound;
using Server.Worker.Extensions;
using System.Diagnostics;

namespace Server.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServerNetworkApp _serverNetworkApp;
    private readonly ServerConfig _serverConfig;
    private readonly WorkerOptions _workerOptions;
    private readonly NetworkPlayerHandlerAdapter _networkPlayerHandler;
    private DateTime _lastHealthCheck = DateTime.UtcNow;
    private const int HEALTH_CHECK_INTERVAL_SECONDS = 60;
    private readonly Stopwatch _updateStopwatch = new Stopwatch();
    private readonly Queue<double> _updateTimings = new Queue<double>();
    private const int UPDATE_TIMINGS_MAX_COUNT = 100;
    private double _lastAverageUpdateTime = 0;

    public Worker(
        ILogger<Worker> logger, 
        IServerNetworkApp serverNetworkApp,
        IOptions<ServerConfig> serverConfig,
        IOptions<WorkerOptions> workerOptions,
        NetworkPlayerHandlerAdapter networkPlayerHandler)
    {
        _logger = logger;
        _serverNetworkApp = serverNetworkApp;
        _serverConfig = serverConfig.Value;
        _workerOptions = workerOptions.Value;
        _networkPlayerHandler = networkPlayerHandler;
    }
    
    private void OnConnectionEvent(ConnectionEvent connectionEvent)
    {
        _logger.LogInformation("Evento de conexão recebido: {Event} para o peerId {PeerId}", connectionEvent, connectionEvent.PeerId);
        
        
    }
    
    private void OnDisconnectionEvent(DisconnectionEvent disconnectionEvent)
    {
        _logger.LogInformation("Evento de desconexão recebido: {Event} para o peerId {PeerId}", disconnectionEvent, disconnectionEvent.PeerId);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _serverNetworkApp.EventBus.Subscribe<ConnectionEvent>(OnConnectionEvent);
        _serverNetworkApp.EventBus.Subscribe<DisconnectionEvent>(OnDisconnectionEvent);
        
        // Register network packet handlers
        _networkPlayerHandler.RegisterHandlers();
        _networkPlayerHandler.RegisterEvents();
        
        _logger.LogInformation("Iniciando servidor de jogo na porta {Port}", _serverConfig.Port);
        bool started = _serverNetworkApp.Start(_serverConfig.Port);
        
        if (!started)
        {
            _logger.LogError("Falha ao iniciar servidor na porta {Port}", _serverConfig.Port);
            throw new InvalidOperationException($"Não foi possível iniciar o servidor na porta {_serverConfig.Port}");
        }
        
        _logger.LogInformation("Servidor iniciado com sucesso na porta {Port}", _serverConfig.Port);
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Loop principal do servidor iniciado");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Mede o tempo de execução do ciclo de atualização
                _updateStopwatch.Restart();
                
                // Atualiza o serviço de rede (processa pacotes, lida com eventos, etc.)
                _serverNetworkApp.Update();
                
                _updateStopwatch.Stop();
                
                // Armazena o tempo de atualização para cálculos de média
                TrackUpdateTime(_updateStopwatch.Elapsed.TotalMilliseconds);
                
                // Verifica se é hora de fazer uma verificação de saúde
                if ((DateTime.UtcNow - _lastHealthCheck).TotalSeconds >= HEALTH_CHECK_INTERVAL_SECONDS)
                {
                    PerformHealthCheck();
                    _lastHealthCheck = DateTime.UtcNow;
                }
                
                // Calcula o tempo real a esperar para manter a taxa de atualização desejada
                int sleepTime = CalculateSleepTime(_updateStopwatch.ElapsedMilliseconds);
                
                // Espera pelo intervalo calculado antes da próxima atualização
                if (sleepTime > 0)
                    await Task.Delay(sleepTime, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Registra exceções não tratadas no loop principal
                _logger.LogError(ex, "Erro no loop principal do servidor");
                
                // Breve pausa para evitar CPU alta em caso de erros repetidos
                await Task.Delay(100, stoppingToken);
            }
        }
    }

    private void TrackUpdateTime(double milliseconds)
    {
        _updateTimings.Enqueue(milliseconds);
        
        if (_updateTimings.Count > UPDATE_TIMINGS_MAX_COUNT)
        {
            _updateTimings.Dequeue();
            
            // Recalcula a média a cada 100 amostras
            _lastAverageUpdateTime = _updateTimings.Average();
        }
    }

    private int CalculateSleepTime(long elapsedMs)
    {
        int targetFrameTime = _workerOptions.UpdateIntervalMs;
        int remainingTime = targetFrameTime - (int)elapsedMs;
        
        // Garantir que não teremos valores negativos 
        // (que causariam atraso na próxima atualização)
        return Math.Max(0, remainingTime);
    }

    private void PerformHealthCheck()
    {
        try
        {
            var connectionCount = _serverNetworkApp.ConnectionManager.GetConnectedPeerCount();
            var uptime = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime());
            var memoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024.0);
            
            // Log basic statistics
            _logger.LogInformation(
                "Status do servidor - Conexões: {ConnectionCount}, Uptime: {Uptime:d\\.hh\\:mm\\:ss}, Memória: {MemoryUsageMB:F2}MB",
                connectionCount,
                uptime,
                memoryUsageMB);
            
            // Log detailed performance metrics if enabled
            if (_workerOptions.EnableDetailedMetrics)
            {
                var cpuUsage = GetCpuUsage();
                
                _logger.LogInformation(
                    "Métricas detalhadas - Tempo médio de atualização: {AvgUpdateTime:F3}ms, " +
                    "CPU: {CpuUsage:F1}%, ThreadCount: {ThreadCount}",
                    _lastAverageUpdateTime,
                    cpuUsage,
                    Process.GetCurrentProcess().Threads.Count);
                
                // Log warning if update time is approaching the target update interval
                if (_lastAverageUpdateTime > _workerOptions.UpdateIntervalMs * 0.8)
                {
                    _logger.LogWarning(
                        "Tempo médio de atualização ({AvgUpdateTime:F3}ms) está próximo do intervalo alvo ({UpdateInterval}ms). " +
                        "Considere otimizar ou aumentar o intervalo.",
                        _lastAverageUpdateTime,
                        _workerOptions.UpdateIntervalMs);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao realizar verificação de saúde do servidor");
        }
    }

    private float GetCpuUsage()
    {
        try
        {
            // Este é um método simplificado para estimar o uso de CPU
            // Uma implementação mais precisa dependeria da plataforma específica
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            // Aguarda um curto período para medir
            Thread.Sleep(100);
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsagePercent = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
            
            return (float)cpuUsagePercent;
        }
        catch
        {
            // Em caso de erro, retornamos -1 para indicar que não foi possível calcular
            return -1;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando processo de desligamento do servidor...");
        
        try
        {
            // Unsubscribe from network events
            _serverNetworkApp.EventBus.Unsubscribe<ConnectionEvent>(OnConnectionEvent);
            _serverNetworkApp.EventBus.Unsubscribe<DisconnectionEvent>(OnDisconnectionEvent);
            
            // Provide a grace period for connections to close properly
            _logger.LogInformation("Enviando notificações de desligamento para {ConnectionCount} clientes conectados...", 
                _serverNetworkApp.ConnectionManager.GetConnectedPeerCount());
            
            // Force-disconnect all remaining peers with a 5 second timeout
            await Task.WhenAny(
                DisconnectAllPeersAsync(),
                Task.Delay(TimeSpan.FromSeconds(5), cancellationToken)
            );
            
            _logger.LogInformation("Desligando serviços de rede...");
            _serverNetworkApp.Stop();
            
            _logger.LogInformation("Servidor desligado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o desligamento do servidor");
        }
        
        await base.StopAsync(cancellationToken);
    }

    private async Task DisconnectAllPeersAsync()
    {
        _serverNetworkApp.Stop();

        _serverNetworkApp.Start(_serverConfig.Port);
    }
}
