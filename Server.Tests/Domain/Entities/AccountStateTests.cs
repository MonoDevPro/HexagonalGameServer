using System;
using Server.Domain.Entities;
using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.Services;
using Moq;
using Server.Domain.Events.Account;
using Xunit;
using Server.Application.Ports.Outbound.Security;

namespace Server.Tests.Domain.Entities;

public class AccountStateTests
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Account _account;
    
    public AccountStateTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        
        _account = new Account("testuser", "password123");
    }
    
    [Fact]
    public void NewAccount_HasCreatedState()
    {
        // Assert
        Assert.Equal(AccountState.Created, _account.State);
        
        // Verificar se o evento AccountCreatedEvent foi gerado
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountCreatedEvent);
    }
    
    [Fact]
    public void Activate_FromCreatedState_ReturnsTrue()
    {
        // Act
        bool result = _account.Activate();
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Activated, _account.State);
        
        // Verificar se o evento AccountStateChangedEvent foi gerado
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountStateChangedEvent stateEvent && 
                                     stateEvent.NewState == AccountState.Activated);
    }
    
    [Fact]
    public void Lock_FromCreatedState_ReturnsTrue()
    {
        // Act
        bool result = _account.Lock();
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Locked, _account.State);
        
        // Verificar se o evento AccountStateChangedEvent foi gerado
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountStateChangedEvent stateEvent && 
                                     stateEvent.NewState == AccountState.Locked);
    }
    
    [Fact]
    public void Suspend_FromCreatedState_ReturnsTrue()
    {
        // Act
        bool result = _account.Suspend(TimeSpan.FromDays(1), "Violação de regras");
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Suspended, _account.State);
        
        // Verificar se os eventos foram gerados
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountStateChangedEvent stateEvent && 
                                     stateEvent.NewState == AccountState.Suspended);
        Assert.Contains(events, e => e is AccountSuspendedEvent suspendEvent && 
                                     suspendEvent.Duration == TimeSpan.FromDays(1) &&
                                     suspendEvent.Reason == "Violação de regras");
    }
    
    [Fact]
    public void Ban_FromCreatedState_ReturnsTrue()
    {
        // Act
        bool result = _account.Ban("Violação grave de regras");
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Banned, _account.State);
        
        // Verificar se os eventos foram gerados
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountStateChangedEvent stateEvent && 
                                     stateEvent.NewState == AccountState.Banned);
        Assert.Contains(events, e => e is AccountBannedEvent banEvent && 
                                     banEvent.Reason == "Violação grave de regras");
    }
    
    [Fact]
    public void Delete_FromCreatedState_ReturnsTrue()
    {
        // Act
        bool result = _account.Delete();
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Deleted, _account.State);
        
        // Verificar se os eventos foram gerados
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is AccountStateChangedEvent stateEvent && 
                                     stateEvent.NewState == AccountState.Deleted);
        Assert.Contains(events, e => e is AccountDeletedEvent);
    }
    
    [Fact]
    public void Activate_FromDeletedState_ReturnsFalse()
    {
        // Arrange
        _account.Delete(); // Primeiro coloca a conta no estado Deleted
        _account.ClearDomainEvents(); // Limpa os eventos para verificar apenas os novos
        
        // Act
        bool result = _account.Activate();
        
        // Assert
        Assert.False(result);
        Assert.Equal(AccountState.Deleted, _account.State); // Estado permanece Deleted
        
        // Verificar se o evento de transição inválida foi gerado
        var events = _account.GetDomainEvents();
        Assert.Contains(events, e => e is InvalidAccountStateTransitionEvent);
    }
    
    [Fact]
    public void GetPossibleStateTransitions_ReturnsCorrectTransitions()
    {
        // Act
        var possibleTransitions = _account.GetPossibleStateTransitions();
        
        // Assert
        Assert.Equal(5, possibleTransitions.Count);
        Assert.Contains(AccountState.Activated, possibleTransitions);
        Assert.Contains(AccountState.Locked, possibleTransitions);
        Assert.Contains(AccountState.Suspended, possibleTransitions);
        Assert.Contains(AccountState.Banned, possibleTransitions);
        Assert.Contains(AccountState.Deleted, possibleTransitions);
    }
    
    [Fact]
    public void Authenticate_WhenAccountIsBanned_ReturnsFalse()
    {
        // Arrange
        _account.Ban("Banned for test");
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        
        // Act
        bool result = _account.Authenticate("password123", _mockPasswordHasher.Object);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Authenticate_WhenAccountIsLocked_ReturnsFalse()
    {
        // Arrange
        _account.Lock();
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        
        // Act
        bool result = _account.Authenticate("password123", _mockPasswordHasher.Object);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Authenticate_WhenAccountIsSuspended_ReturnsFalse()
    {
        // Arrange
        _account.Suspend(TimeSpan.FromDays(1), "Suspended for test");
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        
        // Act
        bool result = _account.Authenticate("password123", _mockPasswordHasher.Object);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Authenticate_WhenAccountIsDeleted_ReturnsFalse()
    {
        // Arrange
        _account.Delete();
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        
        // Act
        bool result = _account.Authenticate("password123", _mockPasswordHasher.Object);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Authenticate_WhenAccountIsActivated_ReturnsTrue()
    {
        // Arrange
        _account.Activate();
        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        
        // Act
        bool result = _account.Authenticate("password123", _mockPasswordHasher.Object);
        
        // Assert
        Assert.True(result);
    }
}