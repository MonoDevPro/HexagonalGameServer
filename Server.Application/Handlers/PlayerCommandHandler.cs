using Microsoft.Extensions.Logging;
using Server.Application.Commands;
using Server.Application.Ports.Inbound;
using Server.Application.Ports.Outbound;
using Server.Application.Services;
using Server.Domain.Entities.Primitives;
using Server.Domain.Events.Player;

namespace Server.Application.Handlers;

public class PlayerCommandHandler : IPlayerCommandHandler
{
    private readonly ILogger<PlayerCommandHandler> _logger;
    private readonly PlayerService _playerService;
    private readonly AccountService _accountService;
    private readonly CharacterService _characterService;
    private readonly IPlayerEventPublisher _playerEventPublisher;

    public PlayerCommandHandler(
        ILogger<PlayerCommandHandler> logger,
        PlayerService playerService,
        AccountService accountService,
        CharacterService characterService,
        IPlayerEventPublisher playerEventPublisher)
    {
        _logger = logger;
        _playerService = playerService;
        _accountService = accountService;
        _characterService = characterService;
        _playerEventPublisher = playerEventPublisher;
    }

    public async Task Handle(AccountCreateCommand command)
    {
        _logger.LogInformation("Handling account creation command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            await _accountService.CreateAccountAsync(command.Username, command.Password);
            
            // Publish success event
            await _playerEventPublisher.PublishAsync(new PlayerAccountCreationSuccessEvent(
                command.connectionId,
                command.Username
            ));
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for connection {ConnectionId}", command.connectionId);
            
            // Publish failure event with error message
            await _playerEventPublisher.PublishAsync(new PlayerAccountCreationFailedEvent(
                command.connectionId, command.Username,
                $"Account creation failed: {ex.Message}"
            ));
        }
    }

    public async Task Handle(AccountLoginCommand command)
    {
        _logger.LogInformation("Handling account login command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            bool isAuthenticated = await _accountService.AuthenticateAsync(command.Username, command.Password);
            
            if (isAuthenticated)
            {
                // Register the player connection
                await _playerService.RegisterPlayerAsync(command.connectionId, command.Username);
                
                // Publish success event
                await _playerEventPublisher.PublishAsync(new PlayerAccountLoginSuccessEvent(
                    command.connectionId,
                    command.Username
                ));
            }
            else
            {
                // Publish failure event for invalid credentials
                await _playerEventPublisher.PublishAsync(new PlayerAccountLoginFailedEvent(
                    command.connectionId, command.Username,
                    "Invalid username or password"
                ));
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to login for connection {ConnectionId}", command.connectionId);
            
            // Publish failure event with error message
            await _playerEventPublisher.PublishAsync(new PlayerAccountLoginFailedEvent(
                command.connectionId, command.Username,
                $"Login failed: {ex.Message}"
            ));
        }
    }

    public async Task Handle(AccountLogoutCommand command)
    {
        _logger.LogInformation("Handling account logout command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player != null)
            {
                await _playerService.UnregisterPlayerAsync(command.connectionId);
                
                // No specific event for account logout in the current domain events
                // Could potentially use PlayerDisconnectedEvent instead
                await _playerEventPublisher.PublishAsync(new PlayerDisconnectedEvent(
                    command.connectionId,
                    "Player logged out"
                ));
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error during logout for connection {ConnectionId}", command.connectionId);
        }
    }

    public async Task Handle(CharacterCreateCommand command)
    {
        _logger.LogInformation("Handling character creation command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player != null)
            {
                // Create character with default values
                var character = await _characterService.CreateCharacterAsync(
                    command.characterName,
                    new Stats { Strength = 10, Agility = 10, Defense = 10 },
                    new Vital { Health = 100, MaxHealth = 100, Mana = 50, MaxMana = 50 },
                    new BoundingBox(0, 0, 32, 32),
                    Server.Domain.Enum.Direction.South,
                    1
                );
                
                // Associate character with account
                await _accountService.AddCharacterAsync(player.Account.Username, character);
                
                // Publish success event
                await _playerEventPublisher.PublishAsync(new PlayerCharacterCreationSuccessEvent(
                    command.connectionId,
                    player.Account.Username,
                    character.Name
                ));
            }
            else
            {
                // Publish failure event - player not found
                await _playerEventPublisher.PublishAsync(new PlayerCharacterCreationFailedEvent(
                    command.connectionId,
                    player.Account.Username,
                    command.characterName,
                    "Character creation failed: You must be logged in"
                ));
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to create character for connection {ConnectionId}", command.connectionId);
            
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);

            // Publish failure event with error message
            await _playerEventPublisher.PublishAsync(new PlayerCharacterCreationFailedEvent(
                command.connectionId,
                player?.Account.Username,
                command.characterName,
                $"Character creation failed: {ex.Message}"
            ));
        }
    }

    public async Task Handle(CharacterLogoutCommand command)
    {
        _logger.LogInformation("Handling character logout command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            await _playerService.DeactivateCharacterAsync(command.connectionId);
            
            // Publish character left world event
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            if (player?.SelectedCharacter != null)
            {
                await _playerEventPublisher.PublishAsync(new PlayerCharacterLeftWorldEvent(
                    command.connectionId,
                    player.SelectedCharacter.Id,
                    player.SelectedCharacter.Name,
                    "Player logged out"
                ));
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error during character logout for connection {ConnectionId}", command.connectionId);
        }
    }

    public async Task Handle(CharacterSelectCommand command)
    {
        _logger.LogInformation("Handling character select command for connection {ConnectionId}", command.connectionId);
        
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player != null)
            {
                bool success = await _playerService.ActivateCharacterAsync(command.connectionId, command.CharacterId);
                
                if (success)
                {
                    var character = await _characterService.GetCharacterByIdAsync(command.CharacterId);
                    
                    // Publish success event
                    await _playerEventPublisher.PublishAsync(new PlayerCharacterSelectSuccessEvent(
                        command.connectionId,
                        command.CharacterId,
                        character.Name,
                        player.Account.Username
                    ));
                    
                    // Publish character entered world event
                    await _playerEventPublisher.PublishAsync(new PlayerCharacterEnteredWorldEvent(
                        command.connectionId,
                        command.CharacterId,
                        character.Name,
                        character.FloorIndex,
                        character.BoundingBox.X,
                        character.BoundingBox.Y
                    ));
                }
                else
                {
                    // Publish failure event - character not found or not owned by player
                    await _playerEventPublisher.PublishAsync(new PlayerCharacterSelectFailedEvent(
                        command.connectionId,
                        command.CharacterId,
                        player.Account.Username,
                        "Character selection failed: Character not found or not owned by you"
                    ));
                }
            }
            else
            {
                // Publish failure event - player not found
                await _playerEventPublisher.PublishAsync(new PlayerCharacterSelectFailedEvent(
                    command.connectionId,
                    command.CharacterId,
                    player.Account.Username,
                    "Character selection failed: You must be logged in"
                ));
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to select character for connection {ConnectionId}", command.connectionId);
            
            var p = _playerService.GetPlayerByConnectionId(command.connectionId);

            // Publish failure event with error message
            await _playerEventPublisher.PublishAsync(new PlayerCharacterSelectFailedEvent(
                command.connectionId,
                command.CharacterId,
                p?.Account.Username,
                $"Character selection failed: {ex.Message}"
            ));
        }
    }

    public async Task Handle(MoveCommand command)
    {
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player?.SelectedCharacter != null)
            {
                await _characterService.MoveCharacterAsync(player.SelectedCharacter.Id, command.Direction);
                
                // Note: No specific event for character movement in current domain events
                // Consider implementing a PlayerCharacterMovedEvent
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to move character for connection {ConnectionId}", command.connectionId);
        }
    }

    public async Task Handle(AttackCommand command)
    {
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player?.SelectedCharacter != null)
            {
                await _characterService.AttackAsync(player.SelectedCharacter.Id, command.TargetId);
                
                // Note: No specific event for attack in current domain events
                // Consider implementing a PlayerCharacterAttackedEvent
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to process attack for connection {ConnectionId}", command.connectionId);
        }
    }

    public async Task Handle(ChatCommand command)
    {
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player != null)
            {
                // Process chat message
                await _playerService.ProcessChatMessageAsync(command.connectionId, command.Message);
                
                // Note: No specific event for chat in current domain events
                // Consider implementing a PlayerChatEvent
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to process chat message for connection {ConnectionId}", command.connectionId);
        }
    }

    public async Task Handle(UseItemCommand command)
    {
        try
        {
            var player = _playerService.GetPlayerByConnectionId(command.connectionId);
            
            if (player?.SelectedCharacter != null)
            {
                // Process item usage
                await _playerService.UseItemAsync(command.connectionId, player.SelectedCharacter.Id, command.ItemId);
                
                // Note: No specific event for item usage in current domain events
                // Consider implementing a PlayerItemUsedEvent
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to use item for connection {ConnectionId}", command.connectionId);
        }
    }
}
