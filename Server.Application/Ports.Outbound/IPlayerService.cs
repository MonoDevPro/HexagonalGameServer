namespace Server.Application.Ports.Outbound;

public interface IPlayerService
{
    /// <summary>
    /// Creates a new player for the given connection ID
    /// </summary>
    Task CreatePlayerAsync(int connectionId);

    Task<bool> CreatePlayerAccountAsync(int connectionId, string username, string password);
    Task<bool> CreatePlayerCharacterAsync(int connectionId, string characterName);

    /// <summary>
    /// Authenticates a player with the given username
    /// </summary>
    Task AuthenticatePlayerAsync(int connectionId, string username, string password);

    /// <summary>
    /// Unregisters a player connection
    /// </summary>
    Task UnregisterPlayerAsync(int connectionId);

    /// <summary>
    /// Handles disconnection of a player
    /// </summary>
    Task HandleDisconnectionAsync(int connectionId, string reason);

    /// <summary>
    /// Seleciona um personagem para o jogador
    /// </summary>
    Task<bool> SelectCharacterAsync(int connectionId, long characterId);

    /// <summary>
    /// Logout do personagem atual do jogador
    /// </summary>
    Task<bool> LogoutCharacterAsync(int connectionId);

    /// <summary>
    /// Processa uma mensagem de chat do jogador
    /// </summary>
    Task ProcessChatMessageAsync(int connectionId, string message);
}