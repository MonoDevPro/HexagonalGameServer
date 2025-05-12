namespace Server.Application.Commands;

public readonly record struct PlayerConnectCommand(int connectionId);

public readonly record struct PlayerDisconnectCommand(int connectionId);

public readonly record struct AccountCreateCommand(int connectionId, string Username, string Password);

public readonly record struct AccountLoginCommand(int connectionId, string Username, string Password);

public readonly record struct AccountLogoutCommand(int connectionId);

public readonly record struct CharacterCreateCommand(int connectionId, string characterName);

public readonly record struct CharacterLogoutCommand(int connectionId);

public readonly record struct CharacterSelectCommand(int connectionId, long CharacterId);

public readonly record struct CharacterChatCommand(int connectionId, string message);