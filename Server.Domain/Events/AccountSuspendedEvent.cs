namespace Server.Domain.Events;

public class AccountSuspendedEvent : DomainEvent
{
    public long AccountId { get; }
    public string Username { get; }
    public TimeSpan Duration { get; }
    public string Reason { get; }

    public AccountSuspendedEvent(long accountId, string username, TimeSpan duration, string reason)
    {
        AccountId = accountId;
        Username = username;
        Duration = duration;
        Reason = reason;
    }
}