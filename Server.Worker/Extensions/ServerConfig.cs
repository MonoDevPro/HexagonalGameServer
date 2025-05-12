namespace Server.Worker.Extensions;

/// <summary>
/// Configuration options for the game server
/// </summary>
public class ServerConfig
{
    /// <summary>
    /// Network port the server will listen on
    /// </summary>
    public int Port { get; set; } = 7777;
    
    /// <summary>
    /// Maximum number of concurrent connections allowed
    /// </summary>
    public int MaxConnections { get; set; } = 100;
    
    /// <summary>
    /// Connection key used for authentication
    /// </summary>
    public string ConnectionKey { get; set; } = "HexagonalGameServer";
}