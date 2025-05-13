namespace Server.AdminMetrics.API.Application.Events;

public abstract class ServerMonitoringEvent
{
    public DateTime Timestamp { get; }

    public ServerMonitoringEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}

public class ServerStatusEvent : ServerMonitoringEvent
{
    public bool IsOnline { get; }
    public DateTime StartTime { get; }
    public TimeSpan Uptime { get; }
    public int ConnectedPlayers { get; }
    public string Version { get; }
    public string Environment { get; }
    public ServerStatusEvent(
        bool isOnline,
        DateTime startTime,
        TimeSpan uptime,
        int connectedPlayers,
        string version,
        string environment)
    {
        IsOnline = isOnline;
        StartTime = startTime;
        Uptime = uptime;
        ConnectedPlayers = connectedPlayers;
        Version = version;
        Environment = environment;
    }
}

public class PlayerStatusEvent : ServerMonitoringEvent
{
    public IReadOnlyCollection<ServerPlayerMonitoring> Players { get; }
    public PlayerStatusEvent(IReadOnlyCollection<ServerPlayerMonitoring> players)
    {
        Players = players;
    }

    public class ServerPlayerMonitoring
    {
        public string PlayerId { get; }
        public string Username { get; }
        public DateTime ConnectedSince { get; }
        public string CurrentZone { get; }

        public ServerPlayerMonitoring(string playerId, string username, DateTime connectedSince, string currentZone)
        {
            PlayerId = playerId;
            Username = username;
            ConnectedSince = connectedSince;
            CurrentZone = currentZone;
        }
    }
}

public class ServerMetricsEvent : ServerMonitoringEvent
{
    public double CpuUsage { get; }
    public long MemoryUsageMb { get; }
    public int ActiveThreads { get; }
    public int RequestsPerMinute { get; }
    public int ConnectionsPerMinute { get; }
    public ServerMetricsEvent(
        double cpuUsage,
        long memoryUsageMb,
        int activeThreads,
        int requestsPerMinute,
        int connectionsPerMinute)
    {
        CpuUsage = cpuUsage;
        MemoryUsageMb = memoryUsageMb;
        ActiveThreads = activeThreads;
        RequestsPerMinute = requestsPerMinute;
        ConnectionsPerMinute = connectionsPerMinute;
    }
}