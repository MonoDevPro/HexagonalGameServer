namespace Server.Worker.Extensions;

/// <summary>
/// Configuration options for the Worker service
/// </summary>
public class WorkerOptions
{
    /// <summary>
    /// Interval in milliseconds between server update cycles
    /// </summary>
    public int UpdateIntervalMs { get; set; } = 15;
    
    /// <summary>
    /// Whether to log detailed performance metrics
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = false;
}