using Microsoft.Extensions.Logging;
using Server.Application.Commands;
using Server.Application.Ports.Inbound;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Services;

namespace Server.Application.Handlers;

public class PlayerCommandHandler : IPlayerCommandHandler
{
    private readonly ILogger<PlayerCommandHandler> _logger;
    private readonly IPlayerServicePort _playerService;

    public PlayerCommandHandler(
        ILogger<PlayerCommandHandler> logger,
        IPlayerServicePort playerService)
    {
        _logger = logger;
        _playerService = playerService;
    }

    public async Task<bool> Handle(PlayerConnectCommand command)
    {
        _logger.LogInformation("Handling player connection command for connection {ConnectionId}", command.ConnectionId);
        
        await _playerService.CreatePlayerAsync(command.ConnectionId);
        return true;
    }
    
    public async Task<bool> Handle(PlayerDisconnectCommand command)
    {
        _logger.LogInformation("Handling player disconnection command for connection {ConnectionId}", command.ConnectionId);
        
        await _playerService.UnregisterPlayerAsync(command.ConnectionId);
        return true;
    }

    public async Task<bool> Handle(PlayerAccountCreateCommand command)
    {
        _logger.LogInformation("Handling account creation command for connection {ConnectionId}", command.ConnectionId);
        
        return await _playerService.CreatePlayerAccountAsync(command.ConnectionId, command.Username, command.Password);
    }

    public async Task<bool> Handle(PlayerAccountLoginCommand command)
    {
        _logger.LogInformation("Handling account login command for connection {ConnectionId}", command.ConnectionId);
        
        await _playerService.AuthenticatePlayerAsync(command.ConnectionId, command.Username, command.Password);
        return true;
    }

    public async Task<bool> Handle(PlayerAccountLogoutCommand command)
    {
        _logger.LogInformation("Handling account logout command for connection {ConnectionId}", command.ConnectionId);
        
        await _playerService.UnregisterPlayerAsync(command.ConnectionId);
        return true;
    }

    public async Task<bool> Handle(PlayerCharacterCreateCommand command)
    {
        _logger.LogInformation("Handling character creation command for connection {ConnectionId}", command.ConnectionId);
        
        return await _playerService.CreatePlayerCharacterAsync(command.ConnectionId, command.CharacterName);
    }

    public async Task<bool> Handle(PlayerCharacterLogoutCommand command)
    {
        _logger.LogInformation("Handling character logout command for connection {ConnectionId}", command.ConnectionId);
        
        return await _playerService.LogoutCharacterAsync(command.ConnectionId);
    }

    public async Task<bool> Handle(PlayerCharacterSelectCommand command)
    {
        _logger.LogInformation("Handling character select command for connection {ConnectionId}", command.ConnectionId);
        
        return await _playerService.SelectCharacterAsync(command.ConnectionId, command.CharacterId);
    }

    public async Task<bool> Handle(PlayerCharacterChatCommand characterChatCommand)
    {
        _logger.LogInformation("Handling character chat command for connection {ConnectionId}", characterChatCommand.ConnectionId);

        await _playerService.ProcessChatMessageAsync(characterChatCommand.ConnectionId, characterChatCommand.Message);
        return true;
    }
}
