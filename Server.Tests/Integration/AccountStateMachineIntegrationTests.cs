using Server.Application.Services;
using Server.Domain.Events;
using Xunit;
using Xunit.Abstractions;
using Server.Application.Ports.Outbound.Security;
using Server.Domain.Enums;
using Server.Infrastructure.Outbound.Persistence.Memory;
using Server.Infrastructure.Outbound.Security;
using Server.Application.Ports.Outbound.Messaging;
using Server.Domain.ValueObjects.Account;

namespace Server.Tests.Integration;

public class AccountStateMachineIntegrationTests
{
    private readonly InMemoryAccountRepository _accountRepository;
    private readonly InMemoryCharacterRepository _characterRepository;
    private readonly IPasswordHasherPort _passwordHasher;
    private readonly TestEventPublisher _eventPublisher;
    private readonly AccountService _accountService;
    private readonly ITestOutputHelper _output;
    
    public AccountStateMachineIntegrationTests(ITestOutputHelper output = null)
    {
        _output = output;
        _accountRepository = new InMemoryAccountRepository();
        _characterRepository = new InMemoryCharacterRepository();
        _passwordHasher = new PasswordHasher();
        _eventPublisher = new TestEventPublisher();
        
        _accountService = new AccountService(
            _accountRepository, 
            _characterRepository, 
            _passwordHasher, 
            _eventPublisher
        );
    }
    
    // Classe auxiliar para monitorar os eventos publicados
    private class TestEventPublisher : IEventPublisherPort
    {
        private readonly List<DomainEvent> _publishedEvents = new List<DomainEvent>();
        
        public IReadOnlyCollection<DomainEvent> PublishedEvents => _publishedEvents.AsReadOnly();
        
        public Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
        {
            _publishedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }

        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
        {
            throw new NotImplementedException();
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
        {
            throw new NotImplementedException();
        }
    }
    
    [Fact]
    public async Task CompleteStateMachineWorkflow_CreatedToDeleted_Success()
    {
        // Arrange
        const string username = "stateMachineTest";
        const string password = "password123";

        var accountCreationOptions = new AccountCreationOptions
        {
            Username = username,
            Password = password,
            InitialState = AccountState.Created
        };
        
        // 1. Criar conta (estado inicial: Created)
        await _accountService.CreateAccountAsync(accountCreationOptions);
        
        var account = await _accountRepository.GetByUsernameAsync(username);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Created, account!.State);

        // 2. Autenticar conta (Created -> Activated)
        var authResult = await _accountService.AuthenticateAsync(username, password);
        
        // 2. Ativar conta (Created -> Activated)
        bool activateResult = await _accountService.ActivateAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(activateResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Activated, account!.State);
        
        // 3. Bloquear conta (Activated -> Locked)
        bool lockResult = await _accountService.LockAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(lockResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Locked, account!.State);
        
        // 4. Reativar conta (Locked -> Activated)
        activateResult = await _accountService.ActivateAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(activateResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Activated, account!.State);
        
        // 5. Suspender conta (Activated -> Suspended)
        bool suspendResult = await _accountService.SuspendAccountAsync(authResult, TimeSpan.FromDays(1), "Teste de suspensão");
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(suspendResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Suspended, account!.State);
        
        // 6. Reativar conta novamente (Suspended -> Activated)
        activateResult = await _accountService.ActivateAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(activateResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Activated, account!.State);
        
        // 7. Banir conta (Activated -> Banned)
        bool banResult = await _accountService.BanAccountAsync(authResult, "Teste de banimento");
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(banResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Banned, account!.State);
        
        // 8. Reativar conta após banimento (Banned -> Activated)
        activateResult = await _accountService.ActivateAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(activateResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Activated, account!.State);
        
        // 9. Excluir conta (Activated -> Deleted)
        bool deleteResult = await _accountService.DeleteAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.True(deleteResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Deleted, account!.State);
        
        // 10. Tentar reativar uma conta excluída (deve falhar)
        activateResult = await _accountService.ActivateAccountAsync(authResult);
        account = await _accountRepository.GetByUsernameAsync(username);
        
        Assert.False(activateResult);
        Assert.NotNull(account);
        Assert.Equal(AccountState.Deleted, account!.State);
        
        // Verificar se eventos foram publicados durante todo o fluxo
        Assert.True(_eventPublisher.PublishedEvents.Count > 0, "Eventos de domínio deveriam ter sido publicados");
        
        // Log dos eventos publicados (opcional, ajuda na depuração)
        if (_output != null)
        {
            _output.WriteLine($"Total de eventos publicados: {_eventPublisher.PublishedEvents.Count}");
            foreach (var evt in _eventPublisher.PublishedEvents)
            {
                _output.WriteLine($"Evento: {evt.GetType().Name}");
            }
        }
    }
    
    [Fact]
    public async Task AccountStateTransitions_AllPossiblePaths_Success()
    {
        // Este teste verifica todas as transições possíveis a partir do estado Created
        
        // 1. Created -> Activated
        await TestStateTransition(
            "test_created_activated",
            null, // Sem estado inicial personalizado, usará Created
            AccountState.Activated,
            expectSuccess: true
        );
        
        // 2. Created -> Locked
        await TestStateTransition(
            "test_created_locked",
            null,
            AccountState.Locked,
            expectSuccess: true
        );
        
        // 3. Created -> Suspended
        await TestStateTransition(
            "test_created_suspended",
            null,
            AccountState.Suspended,
            expectSuccess: true
        );
        
        // 4. Created -> Banned
        await TestStateTransition(
            "test_created_banned",
            null,
            AccountState.Banned,
            expectSuccess: true
        );
        
        // 5. Created -> Deleted
        await TestStateTransition(
            "test_created_deleted",
            null,
            AccountState.Deleted,
            expectSuccess: true
        );
        
        // Transições a partir do estado Activated
        
        // 6. Activated -> Locked
        await TestStateTransition(
            "test_activated_locked",
            AccountState.Activated,
            AccountState.Locked,
            expectSuccess: true
        );
        
        // 7. Activated -> Suspended
        await TestStateTransition(
            "test_activated_suspended",
            AccountState.Activated,
            AccountState.Suspended,
            expectSuccess: true
        );
        
        // 8. Activated -> Banned
        await TestStateTransition(
            "test_activated_banned",
            AccountState.Activated,
            AccountState.Banned,
            expectSuccess: true
        );
        
        // 9. Activated -> Deleted
        await TestStateTransition(
            "test_activated_deleted",
            AccountState.Activated,
            AccountState.Deleted,
            expectSuccess: true
        );
        
        // Transições a partir do estado Locked
        
        // 10. Locked -> Activated
        await TestStateTransition(
            "test_locked_activated",
            AccountState.Locked,
            AccountState.Activated,
            expectSuccess: true
        );
        
        // Transições a partir do estado Suspended
        
        // 11. Suspended -> Activated
        await TestStateTransition(
            "test_suspended_activated",
            AccountState.Suspended,
            AccountState.Activated,
            expectSuccess: true
        );
        
        // Transições a partir do estado Banned
        
        // 12. Banned -> Activated
        await TestStateTransition(
            "test_banned_activated",
            AccountState.Banned,
            AccountState.Activated,
            expectSuccess: true
        );
        
        // 13. Banned -> Deleted
        await TestStateTransition(
            "test_banned_deleted",
            AccountState.Banned,
            AccountState.Deleted,
            expectSuccess: true
        );
        
        // Transições inválidas
        
        // 14. Deleted -> Activated (deve falhar)
        await TestStateTransition(
            "test_deleted_activated",
            AccountState.Deleted,
            AccountState.Activated,
            expectSuccess: false
        );
    }
    
    private async Task TestStateTransition(
        string username,
        AccountState? initialState,
        AccountState targetState,
        bool expectSuccess)
    {
        
        // Criar conta para teste

        var accountCreationOptions = new AccountCreationOptions
        {
            Username = username,
            Password = "password123",
            InitialState = initialState ?? AccountState.Created
        };

        await _accountService.CreateAccountAsync(accountCreationOptions);
        
        // Configurar estado inicial se necessário
        if (initialState.HasValue && initialState.Value != AccountState.Created)
        {
            var authResult = await _accountService.AuthenticateAsync(username, "password123");

            switch (initialState.Value)
            {
                case AccountState.Activated:
                    await _accountService.ActivateAccountAsync(authResult);
                    break;
                case AccountState.Locked:
                    await _accountService.ActivateAccountAsync(authResult); // Ativa primeiro
                    await _accountService.LockAccountAsync(authResult);
                    break;
                case AccountState.Suspended:
                    await _accountService.ActivateAccountAsync(authResult); // Ativa primeiro
                    await _accountService.SuspendAccountAsync(authResult, TimeSpan.FromDays(1), "Teste");
                    break;
                case AccountState.Banned:
                    await _accountService.ActivateAccountAsync(authResult); // Ativa primeiro
                    await _accountService.BanAccountAsync(authResult, "Teste");
                    break;
                case AccountState.Deleted:
                    await _accountService.ActivateAccountAsync(authResult); // Ativa primeiro
                    await _accountService.DeleteAccountAsync(authResult);
                    break;
            }
        }
        
        // Verificar estado atual
        var account = await _accountRepository.GetByUsernameAsync(username);
        Assert.NotNull(account);
        
        if (initialState.HasValue)
        {
            Assert.Equal(initialState.Value, account!.State);
        }
        else
        {
            Assert.Equal(AccountState.Created, account!.State);
        }
        
        // Realizar a transição
        bool success = false;

        var auth = await _accountService.AuthenticateAsync(username, "password123");
        
        switch (targetState)
        {
            case AccountState.Activated:
                success = await _accountService.ActivateAccountAsync(auth);
                break;
            case AccountState.Locked:
                success = await _accountService.LockAccountAsync(auth);
                break;
            case AccountState.Suspended:
                success = await _accountService.SuspendAccountAsync(auth, TimeSpan.FromDays(1), "Teste de transição");
                break;
            case AccountState.Banned:
                success = await _accountService.BanAccountAsync(auth, "Teste de transição");
                break;
            case AccountState.Deleted:
                success = await _accountService.DeleteAccountAsync(auth);
                break;
        }
        
        // Recarregar a conta
        account = await _accountRepository.GetByUsernameAsync(username);
        
        // Validar o resultado
        Assert.Equal(expectSuccess, success);
        
        if (expectSuccess)
        {
            Assert.NotNull(account);
            Assert.Equal(targetState, account!.State);
        }
        else if (account != null)
        {
            // Se esperamos falha, o estado não deveria mudar
            Assert.NotEqual(targetState, account.State);
        }
    }
}