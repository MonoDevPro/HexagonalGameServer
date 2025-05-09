using Server.Domain.Enum;

namespace Server.Application.Commands;

public readonly record struct AccountCreateCommand(int connectionId, string Username, string Password);

public readonly record struct AccountLoginCommand(int connectionId, string Username, string Password);

public readonly record struct AccountLogoutCommand(int connectionId);

public readonly record struct CharacterCreateCommand(int connectionId, string characterName);

public readonly record struct CharacterLogoutCommand(int connectionId);

public readonly record struct CharacterSelectCommand(int connectionId, long CharacterId);

public readonly record struct MoveCommand(int connectionId, Direction Direction);

public readonly record struct AttackCommand(int connectionId, long TargetId);

public readonly record struct ChatCommand(int connectionId, string Message);

public readonly record struct UseItemCommand(int connectionId, int ItemId);
