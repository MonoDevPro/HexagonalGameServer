
namespace Server.AdminMetrics.API.Application.DTOs;

public record ServerStatusDto(
    bool IsOnline,
    DateTime StartTime,
    TimeSpan Uptime,
    int ConnectedPlayers,
    string Version,
    string Environment);

public record ServerPlayerDto(
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
