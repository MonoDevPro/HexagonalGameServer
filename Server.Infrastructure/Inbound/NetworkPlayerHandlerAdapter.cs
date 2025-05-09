using Server.Application.Commands;
using Server.Application.Ports.Inbound;
using Server.Application.Services;
using NetworkCommon.Packets.ServerReceiver.Account;
using NetworkCommon.Packets.ServerReceiver.Character;
using Server.Domain.Enum;
using NetworkHexagonal.Core.Domain.Models;
using NetworkCommon.DTOs.Enum;

namespace Server.Infrastructure.In;

public class NetworkPlayerHandlerAdapter
{
    private readonly IPlayerCommandHandler _playerCommandHandler;

    public NetworkPlayerHandlerAdapter(IPlayerCommandHandler playerCommandHandler)
    {
        _playerCommandHandler = playerCommandHandler;
    }

    // Handlers para pacotes de rede vindos do NetworkCommon

    public Task Handle(RequestAccountCreating packet, PacketContext packetContext)
    {
        var command = new AccountCreateCommand(packetContext.PeerId, packet.AccountName, packet.Password);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestAccountLogging packet, PacketContext packetContext)
    {
        var command = new AccountLoginCommand(packetContext.PeerId, packet.AccountName, packet.Password);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestAccountLogout packet, PacketContext packetContext)
    {
        // Supondo que o username esteja em contexto/session, adapte conforme necessário
        var command = new AccountLogoutCommand(packetContext.PeerId);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestCharacterCreating packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterCreateCommand(packetContext.PeerId, packet.CharacterName);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestCharacterLogout packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterLogoutCommand(packetContext.PeerId);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestCharacterSelection packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new CharacterSelectCommand(packetContext.PeerId, packet.CharacterId);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestCharacterAction packet, PacketContext packetContext)
    {
        switch (packet.NextStateDto)
        {
            case CharacterStateDto.Attacking:
                return _playerCommandHandler.Handle(new AttackCommand(packetContext.PeerId, 0));
            case CharacterStateDto.Walking:
                return _playerCommandHandler.Handle(new MoveCommand(packetContext.PeerId, (Direction)packet.DirectionDto));
            // Adicione outros casos conforme necessário
            default:
                throw new NotImplementedException($"A ação {packet.NextStateDto} não está implementada.");
        }
    }

    public Task Handle(RequestCharacterMessaging packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new ChatCommand(packetContext.PeerId, packet.Message);
        return _playerCommandHandler.Handle(command);
    }

    public Task Handle(RequestCharacterUseItem packet, PacketContext packetContext)
    {
        // Supondo que o username venha de contexto/session, adapte conforme necessário
        var command = new UseItemCommand(packetContext.PeerId, packet.ItemId);
        return _playerCommandHandler.Handle(command);
    }

    // Aqui você implementaria o recebimento da rede, parsing e roteamento para o método correto
}
