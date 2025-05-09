using Server.Domain.Entities;
using Server.Domain.Enum;
using Server.Domain.Services;
using Server.Application.Ports.Outbound.Persistence;
using Server.Application.Ports.Outbound.Security;

namespace Server.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IGameEventPublisher _eventPublisher;

    // Os repositórios aqui injetados são de cenário real, de persistencia em banco de dados.
    public AccountService(
        IAccountRepository accountRepository, 
        ICharacterRepository characterRepository,
        IPasswordHasher passwordHasher, 
        IGameEventPublisher eventPublisher)
    {
        _accountRepository = accountRepository;
        _characterRepository = characterRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
    }

    // Criação e autenticação
    public async Task CreateAccountAsync(string username, string password)
    {
        if (await _accountRepository.ExistsAsync(username))
            throw new InvalidOperationException("Username already exists.");
        
        var hashedPassword = _passwordHasher.HashPassword(password);

        var account = new Account(username, hashedPassword);
        await _accountRepository.AddAsync(account);
        
        await PublishDomainEventsAsync(account);
    }

    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            return false;
        
        var hashedPassword = _passwordHasher.HashPassword(password);

        return account.Authenticate(hashedPassword);
    }
    
    // Gerenciamento de senha
    public async Task ChangePasswordAsync(string username, string newPassword)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
        
        var hashedPassword = _passwordHasher.HashPassword(newPassword);

        account.ChangePassword(hashedPassword);
        await _accountRepository.UpdateAsync(account);
        await PublishDomainEventsAsync(account);
    }
    
    // Gerenciamento do estado da conta
    public async Task<bool> ActivateAccountAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Activate();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> BanAccountAsync(string username, string reason)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Ban(reason);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> LockAccountAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Lock();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> SuspendAccountAsync(string username, TimeSpan duration, string reason)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Suspend(duration, reason);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> DeleteAccountAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Delete();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    // Método para obter os possíveis estados para os quais uma conta pode ir
    public async Task<IReadOnlySet<AccountState>> GetPossibleStateTransitionsAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.GetPossibleStateTransitions();
    }
    
    // Método para obter o estado atual da conta
    public async Task<AccountState> GetAccountStateAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.State;
    }
    
    // Gerenciamento de personagens
    public async Task<IReadOnlyCollection<Character>> GetCharactersAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.Characters;
    }
    
    public async Task AddCharacterAsync(string username, Character character)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        account.AddCharacter(character);
        await _characterRepository.AddAsync(character);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
    }
    
    public async Task RemoveCharacterAsync(string username, long characterId)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        var character = account.GetCharacterById(characterId);
        if (character == null)
            throw new InvalidOperationException("Character not found in this account.");
            
        account.RemoveCharacter(character);
        await _characterRepository.DeleteAsync(character);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
    }
    
    public async Task<Character> GetCharacterByIdAsync(string username, long characterId)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        var character = account.GetCharacterById(characterId);
        if (character == null)
            throw new InvalidOperationException("Character not found in this account.");
            
        return character;
    }
    
    public async Task<Character> GetCharacterByNameAsync(string username, string characterName)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        var character = account.GetCharacterByName(characterName);
        if (character == null)
            throw new InvalidOperationException("Character not found in this account.");
            
        return character;
    }
    
    // Método auxiliar para publicar eventos de domínio
    private async Task PublishDomainEventsAsync(Account account)
    {
        var domainEvents = account.GetDomainEvents();
        
        foreach (var domainEvent in domainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        
        account.ClearDomainEvents();
    }
}