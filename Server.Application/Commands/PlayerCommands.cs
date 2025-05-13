namespace Server.Application.Commands;

public readonly record struct PlayerConnectCommand(int ConnectionId);

public readonly record struct PlayerDisconnectCommand(int ConnectionId);

public readonly record struct PlayerAccountCreateCommand(int ConnectionId, string Username, string Password);

public readonly record struct PlayerAccountLoginCommand(int ConnectionId, string Username, string Password);

public readonly record struct PlayerAccountLogoutCommand(int ConnectionId);

public readonly record struct PlayerCharacterCreateCommand(int ConnectionId, string CharacterName);

public readonly record struct PlayerCharacterLogoutCommand(int ConnectionId);

public readonly record struct PlayerCharacterSelectCommand(int ConnectionId, long CharacterId);

public readonly record struct PlayerCharacterChatCommand(int ConnectionId, string Message);