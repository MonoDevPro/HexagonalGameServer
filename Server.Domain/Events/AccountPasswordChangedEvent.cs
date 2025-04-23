namespace Server.Domain.Events;

public class AccountPasswordChangedEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }

    public AccountPasswordChangedEvent(long accountId, string username)
    {
        AccountId = accountId;
        Username = username;
    }
}