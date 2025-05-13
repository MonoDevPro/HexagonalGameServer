using Microsoft.AspNetCore.Mvc;
using Server.AdminMetrics.API.Application.Commands;
using Server.AdminMetrics.API.Application.DTOs;
using Server.AdminMetrics.API.Application.Events;
using Server.AdminMetrics.API.Application.Ports.Inbound;
using Server.AdminMetrics.API.Application.Ports.Inbound.Messaging;

namespace Server.AdminMetrics.API.Infrastructure.Inbound;

[ApiController]
[Route("api/[controller]")]
public class ServerMonitoringController : ControllerBase
{
    private readonly IServerMonitoringCommandHandler _monitoringCommandHandler;
    private readonly IHttpSubscriber _httpSubscriber;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10); // Increased from 5 to 10 seconds

    public ServerMonitoringController(
        IServerMonitoringCommandHandler monitoringCommandHandler,
        IHttpSubscriber httpSubscriber)
    {
        _monitoringCommandHandler = monitoringCommandHandler;
        _httpSubscriber = httpSubscriber;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetServerStatus()
    {
        var result = await ExecuteMonitoringRequestAsync<ServerStatusEvent>(
            () => _monitoringCommandHandler.Handle(new RequestServerStatusCommand()));
        
        if (result.IsError || result.Value == null)
            return StatusCode(500, result.ErrorMessage ?? "Failed to get server status");
        
        var response = new ServerStatusDto(
            result.Value.IsOnline,
            result.Value.StartTime,
            result.Value.Uptime,
            result.Value.ConnectedPlayers,
            result.Value.Version,
            result.Value.Environment
        );
        
        return Ok(response);
    }

    [HttpGet("players")]
    public async Task<IActionResult> GetOnlinePlayers()
    {
        var result = await ExecuteMonitoringRequestAsync<PlayerStatusEvent>(
            () => _monitoringCommandHandler.Handle(new RequestOnlinePlayersCommand()));
        
        if (result.IsError || result.Value == null)
            return StatusCode(500, result.ErrorMessage ?? "Failed to get player status");

        var response = result.Value.Players.Select(player => new ServerPlayerDto(
            player.PlayerId,
            player.Username,
            player.ConnectedSince,
            player.CurrentZone
        )).ToList();
        
        return Ok(response);
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetServerMetrics()
    {
        var result = await ExecuteMonitoringRequestAsync<ServerMetricsEvent>(
            () => _monitoringCommandHandler.Handle(new RequestServerMetricsCommand()));
        
        if (result.IsError || result.Value == null)
            return StatusCode(500, result.ErrorMessage ?? "Failed to get server metrics");
        
        var response = new ServerMetricsDto(
            result.Value.CpuUsage,
            result.Value.MemoryUsageMb,
            result.Value.ActiveThreads,
            result.Value.RequestsPerMinute,
            result.Value.ConnectionsPerMinute
        );
        
        return Ok(response);
    }
    
    /// <summary>
    /// Método genérico para executar requisições de monitoramento que seguem o padrão:
    /// 1. Assinar um evento
    /// 2. Enviar um comando
    /// 3. Aguardar a resposta ou timeout
    /// </summary>
    private async Task<Result<TEvent>> ExecuteMonitoringRequestAsync<TEvent>(Func<Task> commandAction) 
        where TEvent : class
    {
        // Create a cancellation token source to handle timeouts properly
        using var cts = new CancellationTokenSource(RequestTimeout);
        var tcs = new TaskCompletionSource<TEvent>();
        IAsyncDisposable? unregisterDisposable = null;
        
        try
        {
            // Register cancellation token to complete the TaskCompletionSource
            cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            
            // 1. Assinar o evento antes de fazer qualquer coisa
            unregisterDisposable = await _httpSubscriber.SubscribeAsync<TEvent>(e =>
            {
                // Make sure we only set the result once
                tcs.TrySetResult(e);
                return Task.CompletedTask;
            });
            
            // 2. Enviar o comando - importante fazer isso após ter se inscrito para o evento
            await commandAction();
            
            try
            {
                // 3. Aguardar a resposta com timeout
                var result = await tcs.Task;
                return Result<TEvent>.Success(result);
            }
            catch (OperationCanceledException)
            {
                return Result<TEvent>.Error($"Request timeout while waiting for {typeof(TEvent).Name}");
            }
        }
        catch (Exception ex)
        {
            return Result<TEvent>.Error($"Error processing request: {ex.Message}");
        }
        finally
        {
            // Garantir que a assinatura seja sempre cancelada
            if (unregisterDisposable != null)
            {
                await unregisterDisposable.DisposeAsync();
            }
        }
    }
    
    /// <summary>
    /// Classe para encapsular resultados de operações que podem falhar
    /// </summary>
    private class Result<T> where T : class
    {
        public T? Value { get; }
        public string? ErrorMessage { get; }
        public bool IsError => ErrorMessage != null;
        
        private Result(T? value, string? errorMessage)
        {
            Value = value;
            ErrorMessage = errorMessage;
        }
        
        public static Result<T> Success(T value) => new(value, null);
        public static Result<T> Error(string errorMessage) => new(null, errorMessage);
    }
}
