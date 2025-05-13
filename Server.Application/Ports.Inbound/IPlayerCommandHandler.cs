using System.Threading.Tasks;
using Server.Application.Commands;

namespace Server.Application.Ports.Inbound;

public interface IPlayerCommandHandler
{
    Task<bool> Handle(PlayerConnectCommand command);
    Task<bool> Handle(PlayerDisconnectCommand command);
    Task<bool> Handle(PlayerAccountCreateCommand command);
    Task<bool> Handle(PlayerAccountLoginCommand command);
    Task<bool> Handle(PlayerAccountLogoutCommand command);
    Task<bool> Handle(PlayerCharacterCreateCommand command);
    Task<bool> Handle(PlayerCharacterLogoutCommand command);
    Task<bool> Handle(PlayerCharacterSelectCommand command);
    Task<bool> Handle(PlayerCharacterChatCommand command);
}
