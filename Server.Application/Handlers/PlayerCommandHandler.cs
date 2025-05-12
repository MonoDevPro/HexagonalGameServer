using Microsoft.Extensions.Logging;
using Server.Application.Commands;
using Server.Application.Ports.Inbound;
using Server.Application.Ports.Outbound;
using Server.Application.Services;

namespace Server.Application.Handlers;

public class PlayerCommandHandler : IPlayerCommandHandler
{
    private readonly ILogger<PlayerCommandHandler> _logger;
    private readonly IPlayerService _playerService;

    public PlayerCommandHandler(
        ILogger<PlayerCommandHandler> logger,
        IPlayerService playerService)
    {
        _logger = logger;
        _playerService = playerService;
    }

    public async Task Handle(PlayerConnectCommand command)
    {
        _logger.LogInformation("Handling player connection command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.CreatePlayerAsync(command.connectionId);
    }
    
    public async Task Handle(PlayerDisconnectCommand command)
    {
        _logger.LogInformation("Handling player disconnection command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.UnregisterPlayerAsync(command.connectionId);
    }

    public async Task Handle(AccountCreateCommand command)
    {
        _logger.LogInformation("Handling account creation command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.CreatePlayerAccountAsync(command.connectionId, command.Username, command.Password);
    }

    public async Task Handle(AccountLoginCommand command)
    {
        _logger.LogInformation("Handling account login command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.AuthenticatePlayerAsync(command.connectionId, command.Username, command.Password);
    }

    public async Task Handle(AccountLogoutCommand command)
    {
        _logger.LogInformation("Handling account logout command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.UnregisterPlayerAsync(command.connectionId);
    }

    public async Task Handle(CharacterCreateCommand command)
    {
        _logger.LogInformation("Handling character creation command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.CreatePlayerCharacterAsync(command.connectionId, command.characterName);
    }

    public async Task Handle(CharacterLogoutCommand command)
    {
        _logger.LogInformation("Handling character logout command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.LogoutCharacterAsync(command.connectionId);
    }

    public async Task Handle(CharacterSelectCommand command)
    {
        _logger.LogInformation("Handling character select command for connection {ConnectionId}", command.connectionId);
        
        await _playerService.SelectCharacterAsync(command.connectionId, command.CharacterId);
    }

    public async Task Handle(CharacterChatCommand characterChatCommand)
    {
        _logger.LogInformation($"Handling character chat command for connection {characterChatCommand.connectionId}");

        await _playerService.ProcessChatMessageAsync(characterChatCommand.connectionId, characterChatCommand.message);
    }
}
