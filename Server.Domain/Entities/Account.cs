using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.Services;
using Server.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Domain.Entities;

public class Account : Entity
{
    public string Username { get; private set; }
    private string _passwordHash;
    public AccountState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    // Relação com Character - usando composição para garantir encapsulamento
    private readonly List<Character> _characters = new();
    public IReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    protected Account()
    {
    } // Para uso do ORM

    public Account(string username, string password, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        Username = username;
        _passwordHash = passwordHasher.HashPassword(password);
        State = AccountState.Created;
        CreatedAt = DateTime.UtcNow;
        
        // Publicar evento de criação de conta
        AddDomainEvent(new AccountCreatedEvent(Id, Username));
    }

    public bool Authenticate(string password, IPasswordHasher passwordHasher)
    {
        // Verificar se a conta está em um estado que permite autenticação
        if (State == AccountState.Banned || State == AccountState.Deleted || 
            State == AccountState.Locked || State == AccountState.Suspended)
        {
            return false;
        }
        
        bool isAuthenticated = passwordHasher.VerifyHashedPassword(_passwordHash, password);
        
        if (isAuthenticated)
        {
            LastLoginAt = DateTime.UtcNow;
        }
        
        return isAuthenticated;
    }

    public void ChangePassword(string newPassword, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("New password cannot be empty.", nameof(newPassword));

        _passwordHash = passwordHasher.HashPassword(newPassword);
        
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
        AddDomainEvent(new CharacterAddedToAccountEvent(Id, Username, character.Id, character.Name));
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