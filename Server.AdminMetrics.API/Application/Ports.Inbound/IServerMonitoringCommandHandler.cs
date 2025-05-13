using Server.AdminMetrics.API.Application.Commands;

namespace Server.AdminMetrics.API.Application.Ports.Inbound;

public interface IServerMonitoringCommandHandler
{
    Task Handle(RequestServerStatusCommand command);
    Task Handle(RequestOnlinePlayersCommand command);
    Task Handle(RequestServerMetricsCommand command);
}