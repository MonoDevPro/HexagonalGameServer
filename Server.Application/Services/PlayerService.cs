using Microsoft.Extensions.Logging;
using Server.Application.Factories;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Messaging;
using Server.Domain.Entities;
using Server.Domain.Events.Player.Account;
using Server.Domain.Events.Player.Character;
using Server.Domain.Events.Player.Connection;

namespace Server.Application.Services;

/// <summary>
/// Service responsible for managing active players and their game states
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> _logger;
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;
    private readonly IPlayerCachePort _playerCache;
    private readonly IGameEventPublisher _eventPublisher;

    public PlayerService(
        ILogger<PlayerService> logger,
        IAccountService accountService,
        ICharacterService characterService,
        IPlayerCachePort playerCache,
        IGameEventPublisher eventPublisher)
    {
        _logger = logger;
        _accountService = accountService;
        _characterService = characterService;
        _playerCache = playerCache;
        _eventPublisher = eventPublisher;
    }

    #region Player Connection Management
    
    /// <summary>
    /// Creates a new player for the given connection ID
    /// </summary>
    public async Task CreatePlayerAsync(int connectionId)
    {
        _logger.LogInformation("Creating player for connection {ConnectionId}", connectionId);
        
        // Create new player instance with the connection ID
        var player = new Player(connectionId);
        
        // Add to player cache
        if (!await _playerCache.AddAsync(player))
            return;
        
        await PublishPlayerEventsAsync(player);
    }

    public async Task<bool> CreatePlayerAccountAsync(int connectionId, string username, string password)
    {
        _logger.LogInformation("Creating account for player connection {ConnectionId} with username {Username}",
            connectionId, username);

        // Check if the connection exists
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            throw new InvalidOperationException($"Connection {connectionId} not found.");
        }

        // Check if username is already in use from cache.
        if (await _playerCache.ExistsByUsernameAsync(username))
        {
            player.AddDomainEvent(new PlayerAccountCreationFailedEvent(
                connectionId,
                username,
                "Username already in use"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }

        var defaultAccount = AccountTemplateFactory.CreatePlayerAccount(username, password);

        // Create the account
        var result = await _accountService.CreateAccountAsync(defaultAccount);
        if (!result)
        {
            player.AddDomainEvent(new PlayerAccountCreationFailedEvent(
                connectionId,
                username,
                "Account creation failed"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }
        
        var authentication = 
            await _accountService.AuthenticateAsync(username, password);

        if (authentication is null || !player.Authenticate(authentication))
        {
            player.AddDomainEvent(new PlayerAccountCreationFailedEvent(
                connectionId,
                username,
                "Failed to authenticate player"
            ));
            
            await PublishPlayerEventsAsync(player);
            return false;
        }

        // Atualizar o jogador no cache após autenticação
        await _playerCache.UpdateAsync(player);

        player.AddDomainEvent(new PlayerAccountCreationSuccessEvent(
            connectionId,
            player.AccountAuthentication!.AccountId,
            player.Username
        ));
        
        await PublishPlayerEventsAsync(player);
        return true;
    }

    public async Task<bool> CreatePlayerCharacterAsync(int connectionId, string characterName)
    {
        _logger.LogInformation("Creating character for player connection {ConnectionId} with name {CharacterName}",
            connectionId, characterName);

        // Check if the connection exists
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            throw new InvalidOperationException($"Connection {connectionId} not found.");
        }

        // Check if the player is authenticated
        if (player.AccountAuthentication is null || !player.IsAuthenticated)
        {
            player.AddDomainEvent(new PlayerCharacterCreationFailedEvent(
                connectionId,
                "",
                characterName,
                "Player not authenticated"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }

        var creationOptions = CharacterTemplateFactory.CreateDefault(characterName);

        var character = await _characterService.CreateCharacterAsync(player.AccountAuthentication, creationOptions);
        
        var result = await _accountService.AttachCharacterAsync(
            player.AccountAuthentication, 
            character.Id);

        if (!result)
        {
            player.AddDomainEvent(new PlayerCharacterCreationFailedEvent(
                connectionId,
                player.Username,
                characterName,
                "Character creation failed"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }
        
        await PublishPlayerEventsAsync(player);
        return true;
    }

    /// <summary>
    /// Authenticates a player with the given username
    /// </summary>
    public async Task AuthenticatePlayerAsync(int connectionId, string username, string password)
    {
        _logger.LogInformation("Authenticating player connection {ConnectionId} with username {Username}",
            connectionId, username);

        // Check if the connection exists
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            throw new InvalidOperationException($"Connection {connectionId} not found.");
        }

        // Check if username is already in use by another connection
        var existingPlayer = await _playerCache.GetByUsernameAsync(username);
        if (existingPlayer != null && existingPlayer.ConnectionId != connectionId)
        {
            player.AddDomainEvent(new PlayerAccountLoginFailedEvent(
                connectionId,
                username,
                "Username already connected from another session"
            ));
            await PublishPlayerEventsAsync(player);
            return;
        }

        var accountAuthentication = await _accountService.AuthenticateAsync(username, password);

        if (accountAuthentication is null)
        {
            player.AddDomainEvent(new PlayerAccountLoginFailedEvent(
                connectionId,
                username,
                "Invalid username or password"
            ));
            await PublishPlayerEventsAsync(player);
            return;
        }
        
        // Authenticate the player with the account
        if (!player.Authenticate(accountAuthentication))
        {
            await PublishPlayerEventsAsync(player);
            return;
        }
        
        // Atualizar o jogador no cache
        await _playerCache.UpdateAsync(player);

        player.AddDomainEvent(new PlayerAccountLoginSuccessEvent(
            connectionId,
            username
        ));
        
        await PublishPlayerEventsAsync(player);
    }

    /// <summary>
    /// Unregisters a player connection
    /// </summary>
    public async Task UnregisterPlayerAsync(int connectionId)
    {
        _logger.LogInformation("Unregistering player connection {ConnectionId}", connectionId);

        // Check if the connection exists
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            throw new InvalidOperationException($"Connection {connectionId} not found.");
        }

        player.Logout();
        
        // Capturar eventos antes de remover do cache
        await PublishPlayerEventsAsync(player);

        // Remove from cache
        await _playerCache.RemoveAsync(connectionId);
    }

    /// <summary>
    /// Handles disconnection of a player
    /// </summary>
    public async Task HandleDisconnectionAsync(int connectionId, string reason)
    {
        _logger.LogInformation("Handling disconnection for connection {ConnectionId}: {Reason}", connectionId, reason);

        var player = await _playerCache.GetAsync(connectionId);
        if (player != null)
        {
            // Publish disconnection event
            await _eventPublisher.PublishAsync(new PlayerDisconnectedEvent(connectionId, reason));

            // Unregister the player
            await UnregisterPlayerAsync(connectionId);
        }
    }

    #endregion

    /// <summary>
    /// Seleciona um personagem para o jogador
    /// </summary>
    public async Task<bool> SelectCharacterAsync(int connectionId, long characterId)
    {
        _logger.LogInformation("Selecionando personagem {CharacterId} para a conexão {ConnectionId}", 
            characterId, connectionId);

        // Obter jogador do cache
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            _logger.LogWarning("Não foi possível selecionar personagem: conexão {ConnectionId} não encontrada", connectionId);
            return false;
        }

        // Verificar se o jogador está autenticado
        if (player.AccountAuthentication is null || !player.IsAuthenticated)
        {
            player.AddDomainEvent(new PlayerCharacterSelectFailedEvent(
                connectionId,
                characterId,
                player.Username,
                "Jogador não autenticado"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }

        // Verificar se o personagem pertence ao jogador
        var characters = await _accountService.GetCharactersAsync(player.AccountAuthentication);
        var character = characters.FirstOrDefault(c => c.Id == characterId);
    
        if (character == null)
        {
            player.AddDomainEvent(new PlayerCharacterSelectFailedEvent(
                connectionId,
                characterId,
                player.Username,
                "Personagem não encontrado ou não pertence a esta conta"
            ));
            await PublishPlayerEventsAsync(player);
            return false;
        }

        // Selecionar o personagem
        if (!player.SelectCharacter(character))
        {
            await PublishPlayerEventsAsync(player);
            return false;
        }

        // Atualizar o jogador no cache
        await _playerCache.UpdateAsync(player);

        // Publicar eventos gerados
        await PublishPlayerEventsAsync(player);
    
        return true;
    }

    /// <summary>
    /// Logout do personagem atual do jogador
    /// </summary>
    public async Task<bool> LogoutCharacterAsync(int connectionId)
    {
        _logger.LogInformation("Desativando personagem para a conexão {ConnectionId}", connectionId);

        // Obter jogador do cache
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            _logger.LogWarning("Não foi possível fazer logout do personagem: conexão {ConnectionId} não encontrada", connectionId);
            return false;
        }

        // Usa o método UnselectCharacter da entidade Player para desselecionar o personagem
        player.UnselectCharacter();
        
        // Atualizar o jogador no cache
        await _playerCache.UpdateAsync(player);
        
        // Publicar os eventos gerados
        await PublishPlayerEventsAsync(player);

        return true;
    }

    /// <summary>
    /// Processa uma mensagem de chat do jogador
    /// </summary>
    public async Task ProcessChatMessageAsync(int connectionId, string message)
    {
        _logger.LogInformation("Processando mensagem de chat da conexão {ConnectionId}", connectionId);

        // Obter jogador do cache (operação rápida, sem acesso ao banco)
        var player = await _playerCache.GetAsync(connectionId);
        if (player == null)
        {
            _logger.LogWarning("Não foi possível processar mensagem de chat: conexão {ConnectionId} não encontrada", connectionId);
            return;
        }

        // A entidade Player é responsável por validar e adicionar os eventos apropriados
        bool success = player.SendChatMessage(message);
        
        // Publicar todos os eventos gerados
        await PublishPlayerEventsAsync(player);
    }

    /// <summary>
    /// Publica todos os eventos armazenados na entidade Player
    /// </summary>
    private async Task PublishPlayerEventsAsync(Player player)
    {
        var events = player.GetDomainEvents();
        foreach (var domainEvent in events)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        player.ClearDomainEvents();
    }
}
