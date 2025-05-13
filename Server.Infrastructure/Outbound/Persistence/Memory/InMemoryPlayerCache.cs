// filepath: /home/filipe/Desenvolvimento/NOVOSERVER/HexagonalGameServer/Server.Infrastructure/Outbound/InMemoryPlayerCache.cs

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Cache;
using Server.Domain.Entities;

namespace Server.Infrastructure.Outbound.Persistence.Memory;

/// <summary>
/// Implementação em memória do cache de jogadores
/// </summary>
public class InMemoryPlayerCache : IPlayerCachePort
{
    private readonly ILogger<InMemoryPlayerCache> _logger;
    private readonly ConcurrentDictionary<int, Player> _playersByConnection = new();
    private readonly ConcurrentDictionary<string, Player> _playersByUsername = new();

    public InMemoryPlayerCache(ILogger<InMemoryPlayerCache> logger)
    {
        _logger = logger;
    }

    public Task<Player?> GetAsync(int connectionId)
    {
        _playersByConnection.TryGetValue(connectionId, out var player);
        return Task.FromResult(player);
    }

    public Task<Player?> GetByUsernameAsync(string username)
    {
        _playersByUsername.TryGetValue(username, out var player);
        return Task.FromResult(player);
    }

    public Task<bool> AddAsync(Player player)
    {
        bool success = _playersByConnection.TryAdd(player.ConnectionId, player);
        
        // Se autenticado, adiciona ao dicionário por nome de usuário
        if (success && player.IsAuthenticated)
        {
            _playersByUsername.TryAdd(player.Username, player);
        }
        
        return Task.FromResult(success);
    }

    public Task<bool> UpdateAsync(Player player)
    {
        // Atualiza no dicionário por conexão
        _playersByConnection[player.ConnectionId] = player;
        
        // Se autenticado, atualiza no dicionário por nome de usuário
        if (player.IsAuthenticated)
        {
            _playersByUsername[player.Username] = player;
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> RemoveAsync(int connectionId)
    {
        // Remove do dicionário por conexão
        if (_playersByConnection.TryRemove(connectionId, out var player))
        {
            _playersByUsername.TryRemove(player.Username, out _);
            
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<bool> RemoveByUsernameAsync(string username)
    {
        // Remove do dicionário por nome de usuário
        if (_playersByUsername.TryRemove(username, out var player))
        {
            // Remove também do dicionário por conexão
            _playersByConnection.TryRemove(player.ConnectionId, out _);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<bool> ExistsByConnectionIdAsync(int connectionId)
    {
        return Task.FromResult(_playersByConnection.ContainsKey(connectionId));
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        return Task.FromResult(_playersByUsername.ContainsKey(username));
    }

    public Task<int> GetCountAsync()
    {
        return Task.FromResult(_playersByConnection.Count);
    }

    public Task<IReadOnlyCollection<Player>> GetAllPlayersAsync()
    {
        return Task.FromResult<IReadOnlyCollection<Player>>(_playersByConnection.Values.ToList());
    }

    public Task InvalidatePlayerAsync(int connectionId)
    {
        _logger.LogInformation("Invalidando cache para o jogador com conexão {ConnectionId}", connectionId);
        
        return RemoveAsync(connectionId);
    }

    public Task InvalidatePlayerByUsernameAsync(string username)
    {
        _logger.LogInformation("Invalidando cache para o jogador com nome de usuário {Username}", username);
        
        return RemoveByUsernameAsync(username);
    }
}