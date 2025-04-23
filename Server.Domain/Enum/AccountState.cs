namespace Server.Domain.Enum;

public enum AccountState : byte
{
    None = 0,
    Created = 1,
    Activated = 2,
    Banned = 3,
    Deleted = 4,
    Locked = 5,
    Suspended = 6
}