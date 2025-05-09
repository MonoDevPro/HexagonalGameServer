using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Persistence;
using Server.Domain.Entities;
using Server.Domain.Enum;
using Server.Domain.Events.Player;

namespace Server.Application.Services;

/// <summary>
/// Service responsible for managing active players and their game states
/// </summary>
public class PlayerService
{
    private readonly ILogger<PlayerService> _logger;
    private readonly AccountService _accountService;
    private readonly CharacterService _characterService;
    private readonly IPlayerEventPublisher _eventPublisher;
    
    // Dictionary to track connected players by connection ID
    private readonly ConcurrentDictionary<int, Player> _connectedPlayers = new();
    
    // Dictionary to track players by username (for quick lookups)
    private readonly ConcurrentDictionary<string, Player> _playersByUsername = new();

    public PlayerService(
        ILogger<PlayerService> logger,
        AccountService accountService,
        CharacterService characterService,
        IPlayerEventPublisher eventPublisher)
    {
        _logger = logger;
        _accountService = accountService;
        _characterService = characterService;
        _eventPublisher = eventPublisher;
    }

    #region Player Connection Management

    /// <summary>
    /// Registers a new player connection and associates it with the given username
    /// </summary>
    public async Task<bool> RegisterPlayerAsync(int connectionId, string username)
    {
        _logger.LogInformation("Registering player connection {ConnectionId} for username {Username}", connectionId, username);
        
        // Create new player instance
        var player = new Player(connectionId, username);
        
        // Add to dictionaries
        if (!_connectedPlayers.TryAdd(connectionId, player))
        {
            _logger.LogWarning("Connection ID {ConnectionId} already registered", connectionId);
            return false;
        }
        
        if (!_playersByUsername.TryAdd(username, player))
        {
            // Username already connected, remove the connection ID entry we just added
            _connectedPlayers.TryRemove(connectionId, out _);
            _logger.LogWarning("Username {Username} is already connected from another session", username);
            return false;
        }
        
        // Load player's characters for quick access
        var characters = await _accountService.GetCharactersAsync(username);
        player.SetAvailableCharacters(characters);
        
        _logger.LogInformation("Player registered successfully: {Username} (ConnectionId: {ConnectionId})", 
            username, connectionId);
        
        return true;
    }

    /// <summary>
    /// Unregisters a player connection
    /// </summary>
    public async Task<bool> UnregisterPlayerAsync(int connectionId)
    {
        _logger.LogInformation("Unregistering player connection {ConnectionId}", connectionId);
        
        // Check if the connection exists
        if (!_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            _logger.LogWarning("Cannot unregister connection {ConnectionId}: not found", connectionId);
            return false;
        }
        
        // If player has an active character, handle appropriate cleanup
        if (player.ActiveCharacterId.HasValue)
        {
            await DeactivateCharacterAsync(connectionId);
        }
        
        // Remove from dictionaries
        _connectedPlayers.TryRemove(connectionId, out _);
        _playersByUsername.TryRemove(player.Username, out _);
        
        _logger.LogInformation("Player unregistered successfully: {Username} (ConnectionId: {ConnectionId})", 
            player.Username, connectionId);
        
        return true;
    }
    
    /// <summary>
    /// Handles disconnection of a player
    /// </summary>
    public async Task HandleDisconnectionAsync(int connectionId, string reason)
    {
        _logger.LogInformation("Handling disconnection for connection {ConnectionId}: {Reason}", connectionId, reason);
        
        if (_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            // Publish disconnection event
            await _eventPublisher.PublishAsync(new PlayerDisconnectedEvent(connectionId, reason));
            
            // Unregister the player
            await UnregisterPlayerAsync(connectionId);
        }
    }

    #endregion

    #region Character Management

    /// <summary>
    /// Activates a character for the player
    /// </summary>
    public async Task<bool> ActivateCharacterAsync(int connectionId, long characterId)
    {
        _logger.LogInformation("Activating character {CharacterId} for connection {ConnectionId}", characterId, connectionId);
        
        // Check if the connection exists
        if (!_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            _logger.LogWarning("Cannot activate character: connection {ConnectionId} not found", connectionId);
            return false;
        }
        
        try
        {
            // Verify if the character exists and belongs to this player
            var character = await _accountService.GetCharacterByIdAsync(player.Username, characterId);
            if (character == null)
            {
                _logger.LogWarning("Character {CharacterId} not found or doesn't belong to player {Username}", 
                    characterId, player.Username);
                return false;
            }
            
            // Set the active character
            player.SetActiveCharacter(characterId);
            
            _logger.LogInformation("Character {CharacterId} activated for player {Username}", 
                characterId, player.Username);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating character {CharacterId} for player {Username}", 
                characterId, player.Username);
            return false;
        }
    }
    
    /// <summary>
    /// Deactivates the current character for the player
    /// </summary>
    public async Task<bool> DeactivateCharacterAsync(int connectionId)
    {
        _logger.LogInformation("Deactivating character for connection {ConnectionId}", connectionId);
        
        // Check if the connection exists
        if (!_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            _logger.LogWarning("Cannot deactivate character: connection {ConnectionId} not found", connectionId);
            return false;
        }
        
        if (!player.ActiveCharacterId.HasValue)
        {
            _logger.LogInformation("No active character to deactivate for player {Username}", player.Username);
            return false;
        }
        
        long characterId = player.ActiveCharacterId.Value;
        
        // Clear the active character
        player.ClearActiveCharacter();
        
        _logger.LogInformation("Character {CharacterId} deactivated for player {Username}", 
            characterId, player.Username);
        
        return true;
    }

    #endregion

    #region Game Actions

    /// <summary>
    /// Processes a chat message from a player
    /// </summary>
    public async Task ProcessChatMessageAsync(int connectionId, string message)
    {
        _logger.LogInformation("Processing chat message from connection {ConnectionId}", connectionId);
        
        // Check if the connection exists
        if (!_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            _logger.LogWarning("Cannot process chat message: connection {ConnectionId} not found", connectionId);
            return;
        }
        
        // Handle chat message logic - this could involve broadcasting to nearby players,
        // global chat, party chat, etc. depending on your game design
        
        // For now, we'll just log the message
        _logger.LogInformation("Chat message from {Username}: {Message}", player.Username, message);
        
        // TODO: Implement actual chat message handling based on your game requirements
        // This might involve:
        // 1. Checking for chat commands (e.g., /whisper, /party, /global)
        // 2. Filtering inappropriate content
        // 3. Broadcasting to appropriate recipients
    }

    /// <summary>
    /// Processes an item use action from a player
    /// </summary>
    public async Task UseItemAsync(int connectionId, long characterId, int itemId)
    {
        _logger.LogInformation("Processing item use from connection {ConnectionId}: Item {ItemId}", 
            connectionId, itemId);
        
        // Check if the connection exists
        if (!_connectedPlayers.TryGetValue(connectionId, out var player))
        {
            _logger.LogWarning("Cannot process item use: connection {ConnectionId} not found", connectionId);
            return;
        }
        
        // Check if the character matches the active character
        if (!player.ActiveCharacterId.HasValue || player.ActiveCharacterId != characterId)
        {
            _logger.LogWarning("Cannot use item: character mismatch for player {Username}", player.Username);
            return;
        }
        
        // Handle item use logic
        // TODO: Implement actual item use based on your game requirements
        // This would likely involve:
        // 1. Looking up the item in character inventory
        // 2. Applying item effects
        // 3. Updating character state
        // 4. Notifying nearby players if needed
        
        _logger.LogInformation("Item {ItemId} used by character {CharacterId}", itemId, characterId);
    }

    #endregion

    #region Player Lookup Methods

    /// <summary>
    /// Gets a player by their connection ID
    /// </summary>
    public Player GetPlayerByConnectionId(int connectionId)
    {
        return _connectedPlayers.TryGetValue(connectionId, out var player) ? player : null;
    }

    /// <summary>
    /// Gets a player by their username
    /// </summary>
    public Player GetPlayerByUsername(string username)
    {
        return _playersByUsername.TryGetValue(username, out var player) ? player : null;
    }

    /// <summary>
    /// Gets all currently connected players
    /// </summary>
    public IReadOnlyCollection<Player> GetAllConnectedPlayers()
    {
        return _connectedPlayers.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Checks if a player with the given username is connected
    /// </summary>
    public bool IsPlayerConnected(string username)
    {
        return _playersByUsername.ContainsKey(username);
    }

    /// <summary>
    /// Gets the count of connected players
    /// </summary>
    public int GetConnectedPlayerCount()
    {
        return _connectedPlayers.Count;
    }

    #endregion
}
