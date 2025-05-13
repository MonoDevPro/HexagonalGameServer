using Moq;
using Server.Application.Ports.Outbound.Messaging;
using Server.Application.Ports.Outbound.Persistence;
using Server.Application.Ports.Outbound.Security;
using Server.Application.Services;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.Domain.Events;
using Server.Domain.ValueObjects.Account;
using Xunit;

namespace Server.Tests.Application.Services;

public class AccountServiceStateTests
{
    private readonly Mock<IAccountRepositoryPort> _mockAccountRepository;
    private readonly Mock<ICharacterRepositoryPort> _mockCharacterRepository;
    private readonly Mock<IPasswordHasherPort> _mockPasswordHasher;
    private readonly Mock<IEventPublisherPort> _mockEventPublisher;
    private readonly AccountService _accountService;
    private readonly Account _account;
    
    public AccountServiceStateTests()
    {
        _mockAccountRepository = new Mock<IAccountRepositoryPort>();
        _mockCharacterRepository = new Mock<ICharacterRepositoryPort>();
        _mockPasswordHasher = new Mock<IPasswordHasherPort>();
        _mockEventPublisher = new Mock<IEventPublisherPort>();
        
        _accountService = new AccountService(
            _mockAccountRepository.Object,
            _mockCharacterRepository.Object,
            _mockPasswordHasher.Object,
            _mockEventPublisher.Object
        );
        
        // Configure o mock do password hasher
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        
        var accountCreationOptions = new AccountCreationOptions
        {
            Username = "testuser",
            Password = "password123",
            InitialState = AccountState.Created
        };

        // Crie uma conta para testes
        _account = new Account(accountCreationOptions);
        
        // Configure o mock do repositório para retornar a conta de teste
        _mockAccountRepository.Setup(x => x.GetByUsernameAsync("testuser"))
            .ReturnsAsync(_account);
    }
    
    [Fact]
    public async Task ActivateAccountAsync_ValidAccount_ReturnsTrue()
    {
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act
        bool result = await _accountService.ActivateAccountAsync(authentication);
        
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
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act
        bool result = await _accountService.LockAccountAsync(authentication);
        
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

        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());
        
        // Act
        bool result = await _accountService.SuspendAccountAsync(authentication, duration, reason);
        
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

        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());
        
        // Act
        bool result = await _accountService.BanAccountAsync(authentication, reason);
        
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
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act
        bool result = await _accountService.DeleteAccountAsync(authentication);
        
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
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act
        var possibleTransitions = await _accountService.GetPossibleStateTransitionsAsync(authentication);
        
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

        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());
        
        // Act
        var state = await _accountService.GetAccountStateAsync(authentication);
        
        // Assert
        Assert.Equal(AccountState.Activated, state);
    }
    
    [Fact]
    public async Task ActivateAccountAsync_WhenAccountDeleted_ReturnsFalse()
    {
        // Arrange
        _account.Delete();

        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());
        
        // Act
        bool result = await _accountService.ActivateAccountAsync(authentication);
        
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

        var authentication = await _accountService.AuthenticateAsync("nonexistentuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());
            
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _accountService.ActivateAccountAsync(authentication)
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task BanAccountAsync_EmptyReason_ThrowsException(string reason)
    {
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _accountService.BanAccountAsync(authentication, reason)
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SuspendAccountAsync_EmptyReason_ThrowsException(string reason)
    {
        var authentication = await _accountService.AuthenticateAsync("testuser", "password123");
        Assert.NotNull(authentication);
        Assert.True(authentication.CheckValidation());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _accountService.SuspendAccountAsync(authentication, TimeSpan.FromDays(1), reason)
        );
    }
}