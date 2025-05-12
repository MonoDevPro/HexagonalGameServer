// filepath: /home/filipe/Desenvolvimento/NOVOSERVER/HexagonalGameServer/Server.Application/Ports.Outbound/IPlayerCachePort.cs
using Server.Domain.Entities;

namespace Server.Application.Ports.Outbound;

/// <summary>
/// Interface para o cache de jogadores (port outbound)
/// </summary>
public interface IPlayerCachePort
{
    // Operações básicas de cache
    Task<Player?> GetAsync(int connectionId);
    Task<Player?> GetByUsernameAsync(string username);
    Task<bool> AddAsync(Player player);
    Task<bool> UpdateAsync(Player player);
    Task<bool> RemoveAsync(int connectionId);
    Task<bool> RemoveByUsernameAsync(string username);
    
    // Operações de consulta
    Task<bool> ExistsByConnectionIdAsync(int connectionId);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<int> GetCountAsync();
    Task<IReadOnlyCollection<Player>> GetAllPlayersAsync();
    
    // Invalidação de cache
    Task InvalidatePlayerAsync(int connectionId);
    Task InvalidatePlayerByUsernameAsync(string username);
}