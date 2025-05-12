using NetworkCommon.DTOs;
using NetworkCommon.DTOs.Enum;
using NetworkCommon.DTOs.Primitives;
using NetworkCommon.Packets.Client.Account;
using NetworkCommon.Packets.Client.Character;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Mapping;
using Server.Application.Ports.Outbound.Messaging;
using Server.Application.Ports.Outbound.Persistence;
using Server.Domain.Events.Player;
using Server.Domain.Events.Player.Account;
using Server.Domain.Events.Player.Character;
using Server.Domain.Events.Player.Connection;

namespace Server.Infrastructure.Outbound.Messaging;

/// <summary>
/// Implementation of the IPlayerEventPublisher interface for sending player events to network clients
/// </summary>
public class PlayerEventPublisher : IPlayerEventPublisher<PlayerEvent>
{
    private readonly IPacketSender _packetSender;
    private readonly IPlayerCachePort _playerCachePort;
    private readonly IAccountService _accountService;
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly IDtoMapper _dtoMapper;
    
    public PlayerEventPublisher(
        IPacketSender packetSender, 
        IPlayerCachePort playerCachePort,
        IAccountService accountService,
        IGameEventPublisher gameEventPublisher,
        IDtoMapper dtoMapper)
    {
        _packetSender = packetSender;
        _playerCachePort = playerCachePort;
        _accountService = accountService;
        _gameEventPublisher = gameEventPublisher;
        _dtoMapper = dtoMapper;
    }

    private void Subscribers()
    {
        _gameEventPublisher.Subscribe<PlayerConnectedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerDisconnectedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerAccountCreationSuccessEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerAccountCreationFailedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerAccountLoginSuccessEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerAccountLoginFailedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterCreationSuccessEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterCreationFailedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterSelectSuccessEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterSelectFailedEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterEnteredWorldEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerCharacterLeftWorldEvent>(PublishAsync);
        _gameEventPublisher.Subscribe<PlayerChatEvent>(PublishAsync);
    }

    // Connection events
    public Task PublishAsync(PlayerConnectedEvent eventData)
    {
        // Send player data, etc...
        return Task.CompletedTask;
    }

    public Task PublishAsync(PlayerDisconnectedEvent eventData)
    {
        // Notify friends or party members about disconnection
        // For now just log the event
        Console.WriteLine($"Player disconnected: {eventData.ConnectionId}");
        return Task.CompletedTask;
    }
    
    // Account events
    public Task PublishAsync(PlayerAccountCreationSuccessEvent eventData)
    {
        var packet = new ResponseAccountCreating {
            AccountId = eventData.AccountId,
            Message = "Account created successfully"
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public Task PublishAsync(PlayerAccountCreationFailedEvent eventData)
    {
        var packet = new ResponseAccountCreating {
            AccountId = 0,
            Message = eventData.ErrorMessage
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public async Task PublishAsync(PlayerAccountLoginSuccessEvent eventData)
    {
        var currentPlayer = await _playerCachePort.GetByUsernameAsync(eventData.Username);
        var charactersDto = new List<CharacterDto>();
        
        // Add null check for AccountAuthentication and currentPlayer
        if (currentPlayer != null && currentPlayer.AccountAuthentication != null)
        {
            var characters = await _accountService.GetCharactersAsync(currentPlayer.AccountAuthentication);
            
            // Usar o mapper para converter cada character para CharacterDto
            if (characters != null)
            {
                charactersDto = characters.Select(c => _dtoMapper.MapToDto(c)).ToList();
            }
        }
        
        var packet = new ResponseAccountLogging {
            Username = eventData.Username,
            Characters = charactersDto,
            Message = "Login successful"
        };
        
        _packetSender.SendPacket(eventData.ConnectionId, packet);
    }

    public Task PublishAsync(PlayerAccountLoginFailedEvent eventData)
    {
        var packet = new ResponseAccountLogging {
            Username = eventData.Username,
            Message = eventData.ErrorMessage
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }
    
    // Character events
    public Task PublishAsync(PlayerCharacterCreationSuccessEvent eventData)
    {
        // Criar um DTO mínimo com as informações do evento
        var characterDto = _dtoMapper.CreateMinimalCharacterDto(eventData.CharacterName);
        
        var packet = new ResponseCharacterCreating {
            CharacterId = 0, // Não temos o ID nesse evento
            Character = characterDto
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public Task PublishAsync(PlayerCharacterCreationFailedEvent eventData)
    {
        // Since there's no specific error message field in ResponseCharacterCreating,
        // We could use a special character or create a different packet type
        var packet = new ResponseCharacterCreating {
            CharacterId = 0,
            Character = new CharacterDto() // DTO vazio para indicar falha
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public Task PublishAsync(PlayerCharacterSelectSuccessEvent eventData)
    {
        // Criar um DTO com as informações mínimas do evento
        var characterDto = _dtoMapper.CreateMinimalCharacterDto(eventData.CharacterName, eventData.CharacterId);
        
        var packet = new ResponseCharacterLogging {
            CharacterId = eventData.CharacterId,
            CharacterDto = characterDto
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public Task PublishAsync(PlayerCharacterSelectFailedEvent eventData)
    {
        var packet = new ResponseCharacterLogging {
            CharacterId = 0, // Zero para indicar falha
            CharacterDto = new CharacterDto() // DTO vazio para indicar falha
        };
        
        return Task.FromResult(_packetSender.SendPacket(eventData.ConnectionId, packet));
    }

    public Task PublishAsync(PlayerCharacterEnteredWorldEvent eventData)
    {
        // Criar um DTO com os dados do evento
        var characterDto = new CharacterDto
        {
            Name = eventData.CharacterName,
            FloorIndex = eventData.FloorIndex,
            BoundingBoxDto = new BoundingBoxDto
            {
                X = (int)eventData.PositionX,
                Y = (int)eventData.PositionY,
                Width = 1, // Valores default
                Height = 1
            }
        };
        
        var response = new ResponseCharacterLogging
        {
            CharacterId = eventData.CharacterId,
            CharacterDto = characterDto
        };
        
        // Broadcast to players in vicinity
        return Task.FromResult(_packetSender.Broadcast(response));
    }

    public Task PublishAsync(PlayerCharacterLeftWorldEvent eventData)
    {
        // For character leaving world, we use ResponseCharacterLogout
        var packet = new ResponseCharacterLogout {
            CharacterId = eventData.CharacterId
        };
        
        // Broadcast to players in vicinity
        return Task.FromResult(_packetSender.Broadcast(packet));
    }
    
    public Task PublishAsync(PlayerChatEvent eventData)
    {
        var packet = new ResponseCharacterMessaging {
            CharacterName = eventData.CharacterName ?? eventData.Username,
            Message = eventData.Message
        };
        return Task.FromResult(_packetSender.Broadcast(packet));
    }
}
