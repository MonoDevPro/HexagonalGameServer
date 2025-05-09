using System.Threading.Tasks;
using Server.Application.Commands;

namespace Server.Application.Ports.Inbound;

public interface IPlayerCommandHandler
{
    Task Handle(AccountCreateCommand command);
    Task Handle(AccountLoginCommand command);
    Task Handle(AccountLogoutCommand command);
    Task Handle(CharacterCreateCommand command);
    Task Handle(CharacterLogoutCommand command);
    Task Handle(CharacterSelectCommand command);
    Task Handle(MoveCommand command);
    Task Handle(AttackCommand command);
    Task Handle(ChatCommand command);
    Task Handle(UseItemCommand command);
}
