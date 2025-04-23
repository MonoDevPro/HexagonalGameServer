namespace Server.Domain.Events;

public class AccountDeletedEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }

    public AccountDeletedEvent(long accountId, string username)
    {
        AccountId = accountId;
        Username = username;
    }
}