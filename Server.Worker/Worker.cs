using Microsoft.Extensions.Options;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using Server.Worker.Extensions;

namespace Server.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServerNetworkApp _serverNetworkApp;
    private readonly ServerConfig _serverConfig;
    private readonly WorkerOptions _workerOptions;

    public Worker(
        ILogger<Worker> logger, 
        IServerNetworkApp serverNetworkApp,
        IOptions<ServerConfig> serverConfig,
        IOptions<WorkerOptions> workerOptions)
    {
        _logger = logger;
        _serverNetworkApp = serverNetworkApp;
        _serverConfig = serverConfig.Value;
        _workerOptions = workerOptions.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
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
                // Atualiza o serviço de rede (processa pacotes, lida com eventos, etc.)
                _serverNetworkApp.Update();
                
                // Espera pelo intervalo configurado antes da próxima atualização
                await Task.Delay(_workerOptions.UpdateIntervalMs, stoppingToken);
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Desligando servidor...");
        _serverNetworkApp.Stop();
        _logger.LogInformation("Servidor desligado com sucesso");
        
        return base.StopAsync(cancellationToken);
    }
}
