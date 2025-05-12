using NetworkCommon.Packets.Server.Account;
using NetworkCommon.Packets.Server.Character;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using NetworkHexagonal.Core.Domain.Events.Network;
using NetworkHexagonal.Core.Domain.Models;
using Server.Application.Commands;
using Server.Application.Ports.Inbound;

namespace Server.Infrastructure.Inbound;

public class NetworkPlayerHandlerAdapter
{
    private readonly IServerNetworkApp _serverNetworkApp;
    private readonly IPlayerCommandHandler _playerCommandHandler;

    public NetworkPlayerHandlerAdapter(
        IServerNetworkApp serverNetworkApp, 
        IPlayerCommandHandler playerCommandHandler)
    {
        _serverNetworkApp = serverNetworkApp;
        _playerCommandHandler = playerCommandHandler;
    }
    
    public void RegisterHandlers()
    {
        // Aqui você registraria os handlers para os pacotes de rede
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestAccountCreating>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestAccountLogging>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestAccountLogout>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestCharacterCreating>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestCharacterLogout>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestCharacterSelection>(Handle);
        _serverNetworkApp.PacketRegistry.RegisterHandler<RequestCharacterMessaging>(Handle);
        // Adicione outros handlers conforme necessário
    }

    public void RegisterEvents()
    {
        // Aqui você registraria os eventos de conexão e desconexão
        _serverNetworkApp.EventBus.Subscribe<ConnectionEvent>(HandleConnection);
        _serverNetworkApp.EventBus.Subscribe<DisconnectionEvent>(HandleDisconnection);
    }

    private void HandleConnection(ConnectionEvent connectionEvent)
    {
        var command = new PlayerConnectCommand(connectionEvent.PeerId);
        _playerCommandHandler.Handle(command);
    }
    
    private void HandleDisconnection(DisconnectionEvent disconnectionEvent)
    {
        var command = new PlayerDisconnectCommand(disconnectionEvent.PeerId);
        _playerCommandHandler.Handle(command);
    }

    // Handlers para pacotes de rede vindos do NetworkCommon
    public void Handle(RequestAccountCreating packet, PacketContext packetContext)
    {
        var command = new AccountCreateCommand(packetContext.PeerId, packet.AccountName, packet.Password);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestAccountLogging packet, PacketContext packetContext)
    {
        var command = new AccountLoginCommand(packetContext.PeerId, packet.AccountName, packet.Password);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestAccountLogout packet, PacketContext packetContext)
    {
        // Supondo que o username esteja em contexto/session, adapte conforme necessário
        var command = new AccountLogoutCommand(packetContext.PeerId);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestCharacterCreating packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterCreateCommand(packetContext.PeerId, packet.CharacterName);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestCharacterLogout packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterLogoutCommand(packetContext.PeerId);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestCharacterSelection packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterSelectCommand(packetContext.PeerId, packet.CharacterId);
        _playerCommandHandler.Handle(command);
    }

    public void Handle(RequestCharacterMessaging packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterChatCommand(packetContext.PeerId, packet.Message);
        _playerCommandHandler.Handle(command);
    }

    // Aqui você implementaria o recebimento da rede, parsing e roteamento para o método correto
}
