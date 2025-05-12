using System.Threading.Tasks;
using Server.Application.Commands;

namespace Server.Application.Ports.Inbound;

public interface IPlayerCommandHandler
{
    Task Handle(PlayerConnectCommand command);
    Task Handle(PlayerDisconnectCommand command);
    Task Handle(AccountCreateCommand command);
    Task Handle(AccountLoginCommand command);
    Task Handle(AccountLogoutCommand command);
    Task Handle(CharacterCreateCommand command);
    Task Handle(CharacterLogoutCommand command);
    Task Handle(CharacterSelectCommand command);
    Task Handle(CharacterChatCommand command);
}
