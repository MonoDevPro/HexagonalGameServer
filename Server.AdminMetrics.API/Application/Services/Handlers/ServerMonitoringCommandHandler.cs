using Server.AdminMetrics.API.Application.Commands;
using Server.AdminMetrics.API.Application.Ports.Inbound;
using Server.AdminMetrics.API.Application.Ports.Outbound;

namespace Server.AdminMetrics.API.Application.Services.Handlers;

public class ServerMonitoringCommandHandler : IServerMonitoringCommandHandler
{
    private readonly ILogger<ServerMonitoringCommandHandler> _logger;
    private readonly IServerMonitoringPort _serverMonitoringPort;

    public ServerMonitoringCommandHandler(
        ILogger<ServerMonitoringCommandHandler> logger,
        IServerMonitoringPort monitoringService)
    {
        _logger = logger;
        _serverMonitoringPort = monitoringService;
    }

    public Task Handle(RequestServerStatusCommand command)
    {
        return _serverMonitoringPort.GetServerStatusAsync();
    }

    public Task Handle(RequestOnlinePlayersCommand command)
    {
        return _serverMonitoringPort.GetOnlinePlayersAsync();
    }

    public Task Handle(RequestServerMetricsCommand command)
    {
        return _serverMonitoringPort.GetServerMetricsAsync();
    }
}
