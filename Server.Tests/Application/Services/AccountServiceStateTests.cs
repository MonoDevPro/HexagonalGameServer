using System;
using System.Threading.Tasks;
using Moq;
using Server.Application.Services;
using Server.Domain.Entities;
using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.Repositories;
using Server.Domain.Services;
using Xunit;

namespace Server.Tests.Application.Services;

public class AccountServiceStateTests
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IGameEventPublisher> _mockEventPublisher;
    private readonly AccountService _accountService;
    private readonly Account _account;
    
    public AccountServiceStateTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockEventPublisher = new Mock<IGameEventPublisher>();
        
        _accountService = new AccountService(
            _mockAccountRepository.Object,
            _mockCharacterRepository.Object,
            _mockPasswordHasher.Object,
            _mockEventPublisher.Object
        );
        
        // Configure o mock do password hasher
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        
        // Crie uma conta para testes
        _account = new Account("testuser", "password123", _mockPasswordHasher.Object);
        
        // Configure o mock do repositório para retornar a conta de teste
        _mockAccountRepository.Setup(x => x.GetByUsernameAsync("testuser"))
            .ReturnsAsync(_account);
    }
    
    [Fact]
    public async Task ActivateAccountAsync_ValidAccount_ReturnsTrue()
    {
        // Act
        bool result = await _accountService.ActivateAccountAsync("testuser");
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Activated, _account.State);
        
        // Verificar se o repositório foi chamado para atualizar a conta
        _mockAccountRepository.Verify(r => r.UpdateAsync(_account), Times.Once);
        
        // Verificar se o publicador de eventos foi chamado para cada evento de domínio
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<DomainEvent>()),
            Times.AtLeastOnce()
        );
    }
    
    [Fact]
    public async Task LockAccountAsync_ValidAccount_ReturnsTrue()
    {
        // Act
        bool result = await _accountService.LockAccountAsync("testuser");
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Locked, _account.State);
        
        // Verificar se o repositório foi chamado para atualizar a conta
        _mockAccountRepository.Verify(r => r.UpdateAsync(_account), Times.Once);
        
        // Verificar se o publicador de eventos foi chamado
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<DomainEvent>()),
            Times.AtLeastOnce()
        );
    }
    
    [Fact]
    public async Task SuspendAccountAsync_ValidAccount_ReturnsTrue()
    {
        // Arrange
        var duration = TimeSpan.FromDays(3);
        var reason = "Violação de regras";
        
        // Act
        bool result = await _accountService.SuspendAccountAsync("testuser", duration, reason);
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Suspended, _account.State);
        
        // Verificar se o repositório foi chamado para atualizar a conta
        _mockAccountRepository.Verify(r => r.UpdateAsync(_account), Times.Once);
        
        // Verificar se o publicador de eventos foi chamado
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<DomainEvent>()),
            Times.AtLeastOnce()
        );
    }
    
    [Fact]
    public async Task BanAccountAsync_ValidAccount_ReturnsTrue()
    {
        // Arrange
        var reason = "Violação grave de regras";
        
        // Act
        bool result = await _accountService.BanAccountAsync("testuser", reason);
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Banned, _account.State);
        
        // Verificar se o repositório foi chamado para atualizar a conta
        _mockAccountRepository.Verify(r => r.UpdateAsync(_account), Times.Once);
        
        // Verificar se o publicador de eventos foi chamado
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<DomainEvent>()),
            Times.AtLeastOnce()
        );
    }
    
    [Fact]
    public async Task DeleteAccountAsync_ValidAccount_ReturnsTrue()
    {
        // Act
        bool result = await _accountService.DeleteAccountAsync("testuser");
        
        // Assert
        Assert.True(result);
        Assert.Equal(AccountState.Deleted, _account.State);
        
        // Verificar se o repositório foi chamado para atualizar a conta
        _mockAccountRepository.Verify(r => r.UpdateAsync(_account), Times.Once);
        
        // Verificar se o publicador de eventos foi chamado
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<DomainEvent>()),
            Times.AtLeastOnce()
        );
    }
    
    [Fact]
    public async Task GetPossibleStateTransitionsAsync_ReturnsCorrectStates()
    {
        // Act
        var possibleTransitions = await _accountService.GetPossibleStateTransitionsAsync("testuser");
        
        // Assert
        Assert.NotNull(possibleTransitions);
        Assert.Equal(5, possibleTransitions.Count);
        Assert.Contains(AccountState.Activated, possibleTransitions);
        Assert.Contains(AccountState.Locked, possibleTransitions);
        Assert.Contains(AccountState.Suspended, possibleTransitions);
        Assert.Contains(AccountState.Banned, possibleTransitions);
        Assert.Contains(AccountState.Deleted, possibleTransitions);
    }
    
    [Fact]
    public async Task GetAccountStateAsync_ReturnsCorrectState()
    {
        // Arrange
        _account.Activate();
        
        // Act
        var state = await _accountService.GetAccountStateAsync("testuser");
        
        // Assert
        Assert.Equal(AccountState.Activated, state);
    }
    
    [Fact]
    public async Task ActivateAccountAsync_WhenAccountDeleted_ReturnsFalse()
    {
        // Arrange
        _account.Delete();
        
        // Act
        bool result = await _accountService.ActivateAccountAsync("testuser");
        
        // Assert
        Assert.False(result);
        Assert.Equal(AccountState.Deleted, _account.State);
    }
    
    [Fact]
    public async Task ActivateAccountAsync_AccountNotFound_ThrowsException()
    {
        // Arrange
        _mockAccountRepository.Setup(x => x.GetByUsernameAsync("nonexistentuser"))
            .ReturnsAsync((Account)null!);
            
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _accountService.ActivateAccountAsync("nonexistentuser")
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task BanAccountAsync_EmptyReason_ThrowsException(string reason)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _accountService.BanAccountAsync("testuser", reason)
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SuspendAccountAsync_EmptyReason_ThrowsException(string reason)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _accountService.SuspendAccountAsync("testuser", TimeSpan.FromDays(1), reason)
        );
    }
}