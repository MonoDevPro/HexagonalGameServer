using Microsoft.AspNetCore.Mvc;
using Server.Application.Ports.Inbound;

namespace Server.Infrastructure.Inbound.Http;

[ApiController]
[Route("api/[controller]")]
public class ServerMonitoringController : ControllerBase
{
    private readonly IServerMonitoringPort _monitoringService;
    
    public ServerMonitoringController(IServerMonitoringPort monitoringService)
    {
        _monitoringService = monitoringService;
    }
    
    [HttpGet("status")]
    public async Task<IActionResult> GetServerStatus()
    {
        var status = await _monitoringService.GetServerStatusAsync();
        return Ok(status);
    }
    
    [HttpGet("players")]
    public async Task<IActionResult> GetOnlinePlayers()
    {
        var players = await _monitoringService.GetOnlinePlayersAsync();
        return Ok(players);
    }
    
    [HttpGet("metrics")]
    public async Task<IActionResult> GetServerMetrics()
    {
        var metrics = await _monitoringService.GetServerMetricsAsync();
        return Ok(metrics);
    }
}
