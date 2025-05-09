using Server.Application.Ports.Outbound.Persistence;
using Server.Domain.Entities;

namespace Server.Infrastructure.Persistence;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<long, Account> _accounts = new();
    private readonly Dictionary<string, Account> _accountsByUsername = new(StringComparer.OrdinalIgnoreCase);
    private long _nextId = 1;

    public Task<Account?> GetByIdAsync(long id)
    {
        _accounts.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<Account?> GetByUsernameAsync(string username)
    {
        _accountsByUsername.TryGetValue(username, out var account);
        return Task.FromResult(account);
    }

    public Task AddAsync(Account account)
    {
        // Simular geração de ID
        var field = typeof(Entity).GetField("Id", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (field != null)
        {
            field.SetValue(account, _nextId++);
        }

        _accounts[account.Id] = account;
        _accountsByUsername[account.Username] = account;
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Account account)
    {
        if (_accounts.ContainsKey(account.Id))
        {
            _accounts[account.Id] = account;
            
            // Atualizar o dicionário por username caso o username tenha mudado
            var oldUsername = _accountsByUsername.FirstOrDefault(x => x.Value.Id == account.Id).Key;
            if (oldUsername != null && oldUsername != account.Username)
            {
                _accountsByUsername.Remove(oldUsername);
            }
            
            _accountsByUsername[account.Username] = account;
        }
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Account account)
    {
        _accounts.Remove(account.Id);
        _accountsByUsername.Remove(account.Username);
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string username)
    {
        return Task.FromResult(_accountsByUsername.ContainsKey(username));
    }
}