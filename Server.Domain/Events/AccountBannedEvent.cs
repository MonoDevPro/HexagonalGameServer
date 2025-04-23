namespace Server.Domain.Events;

public class AccountBannedEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }
    public string Reason { get; }

    public AccountBannedEvent(long accountId, string username, string reason)
    {
        AccountId = accountId;
        Username = username;
        Reason = reason;
    }
}