using Server.Domain.Enums;
using Server.Domain.ValueObjects;
using Server.Domain.Events.Account;
using Server.Domain.Events.Character;
using Server.Domain.Policies;
using Server.Domain.ValueObjects.Account;

namespace Server.Domain.Entities;

public class Account : Entity
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public AccountState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    
    // Relação com Character - usando composição para garantir encapsulamento
    private readonly List<Character> _characters = new();
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    protected Account()
    {
    } // Para uso do ORM

    public Account(AccountCreationOptions options)
    {
        options.ValidateAndThrow();
        
        Username = options.Username;
        PasswordHash = options.Password;
        State = options.InitialState;
        CreatedAt = DateTime.UtcNow;
        
        // Publicar evento de criação de conta
        AddDomainEvent(new AccountCreatedEvent(Id, Username));
    }

    public bool Authenticate(string hashedPassword)
    {
        // Verificar se a conta está em um estado que permite autenticação
        if (State == AccountState.Banned || State == AccountState.Deleted || 
            State == AccountState.Locked || State == AccountState.Suspended)
        {
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(hashedPassword));
        bool isAuthenticated = PasswordHash.Equals(hashedPassword);
        
        if (isAuthenticated)
        {
            LastLoginAt = DateTime.UtcNow;
        }
        
        return isAuthenticated;
    }

    public void ChangePassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("New password cannot be empty.", nameof(hashedPassword));

        PasswordHash = hashedPassword;
        
        // Podemos adicionar um evento de domínio para notificar a mudança de senha
        AddDomainEvent(new AccountPasswordChangedEvent(Id, Username));
    }
    
    // Método centralizado para gerenciar a transição de estados
    private bool TryChangeState(AccountState targetState, string failureReason = "Invalid state transition")
    {
        if (AccountStateTransition.IsValidTransition(State, targetState))
        {
            AccountState oldState = State;
            State = targetState;
            
            // Adicionar evento somente se a transição aconteceu para um estado diferente
            if (oldState != targetState)
            {
                AddDomainEvent(new AccountStateChangedEvent(oldState, targetState, Id, Username, failureReason));
            }
            
            return true;
        }
        
        // Transição inválida - registrar um evento de problema
        AddDomainEvent(new InvalidAccountStateTransitionEvent(
            State, 
            targetState, 
            Id, 
            Username,
            failureReason));
            
        return false;
    }
    
    // Métodos para gerenciar o estado da conta
    public bool Activate()
    {
        string reason = $"Cannot activate account in state {State}";
        return TryChangeState(AccountState.Activated, reason);
    }
    
    public bool Ban(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Ban reason cannot be empty", nameof(reason));
            
        bool changed = TryChangeState(AccountState.Banned, $"Cannot ban account in state {State}");
        
        if (changed)
        {
            AddDomainEvent(new AccountBannedEvent(Id, Username, reason));
        }
        
        return changed;
    }
    
    public bool Lock()
    {
        string reason = $"Cannot lock account in state {State}";
        return TryChangeState(AccountState.Locked, reason);
    }
    
    public bool Suspend(TimeSpan duration, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason cannot be empty", nameof(reason));
            
        bool changed = TryChangeState(AccountState.Suspended, $"Cannot suspend account in state {State}");
        
        if (changed)
        {
            AddDomainEvent(new AccountSuspendedEvent(Id, Username, duration, reason));
        }
        
        return changed;
    }
    
    public bool Delete()
    {
        string reason = $"Cannot delete account in state {State}";
        bool changed = TryChangeState(AccountState.Deleted, reason);
        
        if (changed)
        {
            AddDomainEvent(new AccountDeletedEvent(Id, Username));
        }
        
        return changed;
    }
    
    // Método para obter os estados possíveis para esta conta
    public IReadOnlySet<AccountState> GetPossibleStateTransitions()
    {
        return AccountStateTransition.GetPossibleTransitions(State);
    }
    
    // Métodos para gerenciar personagens
    public void AddCharacter(Character character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));
            
        if (_characters.Any(c => c.Name.Equals(character.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Character with name '{character.Name}' already exists in this account.");
            
        _characters.Add(character);
        
        // Podemos adicionar um evento de domínio para notificar a adição de um personagem
        AddDomainEvent(new CharacterAddedToAccountEvent(Id, character.Id));
    }
    
    public void RemoveCharacter(Character character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));
            
        if (_characters.Remove(character))
        {
            // Podemos adicionar um evento de domínio para notificar a remoção de um personagem
            AddDomainEvent(new CharacterRemovedFromAccountEvent(Id, Username, character.Id, character.Name));
        }
    }
    
    public Character? GetCharacterById(long characterId)
    {
        return _characters.FirstOrDefault(c => c.Id == characterId);
    }
    
    public Character? GetCharacterByName(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            throw new ArgumentException("Character name cannot be empty.", nameof(characterName));
            
        return _characters.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
    }
}