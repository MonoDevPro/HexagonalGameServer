namespace Server.Domain.Events.Account;

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