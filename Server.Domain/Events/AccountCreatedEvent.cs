namespace Server.Domain.Events;

public class AccountCreatedEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }

    public AccountCreatedEvent(long accountId, string username)
    {
        AccountId = accountId;
        Username = username;
    }
}