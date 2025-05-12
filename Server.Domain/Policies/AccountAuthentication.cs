using Server.Domain.Enums;

namespace Server.Domain.Policies;

public class AccountAuthentication
{
    public long AccountId { get; }
    public string Username { get; }
    public AccountState AccountState { get; }
    public DateTime AuthenticatedAt { get; }
    public TimeSpan TimeToExpire { get; }
    public DateTime ExpiresAt => AuthenticatedAt + TimeToExpire;
    private AccountAuthentication(
        long accountId, 
        string username, 
        AccountState accountState,
        TimeSpan timeToExpire)
    {
        if (accountId <= 0)
            throw new ArgumentException("ID da conta deve ser positivo", nameof(accountId));
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Nome de usuário não pode ser vazio", nameof(username));
        
        if (timeToExpire <= TimeSpan.Zero)
            throw new ArgumentException("Tempo de expiração deve ser positivo", nameof(timeToExpire));
        
        AccountId = accountId;
        Username = username;
        AccountState = accountState;
        AuthenticatedAt = DateTime.UtcNow;
        TimeToExpire = timeToExpire;
    }
        
    // Factory method estático
    public static AccountAuthentication Create(
        long accountId, 
        string username, 
        AccountState accountState,
        TimeSpan timeToExpire)
    {
        return new AccountAuthentication(accountId, username, accountState, timeToExpire);
    }
        
    public bool CheckValidation()
    {
        // Verificar se está expirado
        if (DateTime.UtcNow > AuthenticatedAt + TimeToExpire)
            return false;

        // Verificar se o estado da conta permite autenticação
        return AccountState != AccountState.Banned &&
               AccountState != AccountState.Deleted &&
               AccountState != AccountState.Locked &&
               AccountState != AccountState.Suspended;
    }
    public bool Equals(AccountAuthentication other)
    {
        return AccountId == other.AccountId &&
               Username == other.Username &&
               AccountState == other.AccountState;
    }
    public override bool Equals(object? obj) => 
        obj is AccountAuthentication other && Equals(other);
    public override int GetHashCode() => 
        HashCode.Combine(AccountId, Username, AccountState);
        
    public override string ToString() => 
        $"Autenticação: {Username} (ID: {AccountId}) - Estado: {AccountState}, Expira em: {ExpiresAt}";
}