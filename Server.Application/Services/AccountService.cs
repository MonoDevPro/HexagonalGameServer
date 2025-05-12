using Server.Domain.Entities;
using Server.Application.Ports.Outbound.Persistence;
using Server.Application.Ports.Outbound.Security;
using Server.Domain.ValueObjects;
using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Messaging;
using Server.Domain.Enums;
using Server.Domain.Policies;
using Server.Domain.ValueObjects.Account;

namespace Server.Application.Services;

public class AccountService : IAccountService
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

    // Criação
    public async Task<bool> CreateAccountAsync(AccountCreationOptions options)
    {
        var username = options.Username;
        var password = options.Password;
        
        if (await _accountRepository.ExistsAsync(username))
            throw new InvalidOperationException("Username already exists.");
        
        var hashedPassword = _passwordHasher.HashPassword(password);

        var account = new Account(options);
        
        account.ChangePassword(hashedPassword);
        
        await _accountRepository.AddAsync(account);
        
        await PublishDomainEventsAsync(account);

        return true;
    }

    public async Task<AccountAuthentication?> AuthenticateAsync(string username, string password)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
            return null;
        
        var hashedPassword = _passwordHasher.HashPassword(password);

        if (!account.Authenticate(hashedPassword))
            return null;
        
        return AccountAuthentication.Create(account.Id, account.Username, account.State, TimeSpan.FromMinutes(5));
    }
    
    // Gerenciamento de senha
    public async Task<bool> ChangePasswordAsync(AccountAuthentication authentication, string newPassword)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
        
        var hashedPassword = _passwordHasher.HashPassword(newPassword);

        account.ChangePassword(hashedPassword);
        await _accountRepository.UpdateAsync(account);
        await PublishDomainEventsAsync(account);
        return true;
    }
    
    // Gerenciamento do estado da conta
    public async Task<bool> ActivateAccountAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Activate();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> BanAccountAsync(AccountAuthentication authentication, string reason)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Ban(reason);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> LockAccountAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Lock();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> SuspendAccountAsync(AccountAuthentication authentication, TimeSpan duration, string reason)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Suspend(duration, reason);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    public async Task<bool> DeleteAccountAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        bool success = account.Delete();
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        
        return success;
    }
    
    // Método para obter os possíveis estados para os quais uma conta pode ir
    public async Task<IReadOnlySet<AccountState>> GetPossibleStateTransitionsAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.GetPossibleStateTransitions();
    }
    
    // Método para obter o estado atual da conta
    public async Task<AccountState> GetAccountStateAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.State;
    }
    
    // Gerenciamento de personagens
    public async Task<IReadOnlyCollection<Character>> GetCharactersAsync(AccountAuthentication authentication)
    {
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        return account.Characters;
    }
    
    public async Task<bool> AttachCharacterAsync(AccountAuthentication authentication, long characterId)
    {
        if (!authentication.CheckValidation())
            throw new InvalidOperationException("Invalid authentication.");
        
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");

        if (!await _characterRepository.ExistsAsync(characterId))
            throw new InvalidOperationException($"Character with id '{characterId}' not exists.");
        
        var character = await _characterRepository.GetByIdAsync(characterId);
            
        account.AddCharacter(character!);
        await _characterRepository.AddAsync(character!);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);
        return true;
    }
    
    public async Task<bool> DetachCharacterAsync(AccountAuthentication authentication, long characterId)
    {
        if (!authentication.CheckValidation())
            throw new InvalidOperationException("Invalid authentication.");
        
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        var character = account.GetCharacterById(characterId);
        if (character == null)
            throw new InvalidOperationException("Character not found in this account.");
            
        account.RemoveCharacter(character);
        await _characterRepository.DeleteAsync(character);
        await _accountRepository.UpdateAsync(account);
        
        await PublishDomainEventsAsync(account);

        return true;
    }
    
    public async Task<Character> GetCharacterByIdAsync(AccountAuthentication authentication, long characterId)
    {
        if (!authentication.CheckValidation())
            throw new InvalidOperationException("Invalid authentication.");
        
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
        if (account == null)
            throw new InvalidOperationException("Account not found.");
            
        var character = account.GetCharacterById(characterId);
        if (character == null)
            throw new InvalidOperationException("Character not found in this account.");
            
        return character;
    }
    
    public async Task<Character> GetCharacterByNameAsync(AccountAuthentication authentication, string characterName)
    {
        if (!authentication.CheckValidation())
            throw new InvalidOperationException("Invalid authentication.");
        
        var account = await _accountRepository.GetByUsernameAsync(authentication.Username);
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