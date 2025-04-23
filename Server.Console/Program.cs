using Server.Application.Services;
using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Primitives;
using Server.Infrastructure.Messaging;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Security;

namespace Server.Console;

public class Program
{
    static async Task Main(string[] args)
    {
        // Configuração da infraestrutura
        var accountRepository = new InMemoryAccountRepository();
        var characterRepository = new InMemoryCharacterRepository();
        var passwordHasher = new PasswordHasher();
        var eventPublisher = new InMemoryGameEventPublisher();
        
        // Configuração de serviços da aplicação
        var accountService = new AccountService(accountRepository, characterRepository, passwordHasher, eventPublisher);
        var characterService = new CharacterService(characterRepository, eventPublisher);
        
        // Registrar manipuladores de eventos
        RegisterEventHandlers(eventPublisher);
        
        // Validar a máquina de estados das contas
        await ValidateAccountStateMachineAsync(accountService, accountRepository);
        
        // Demonstração do fluxo do jogo
        // await DemoGameFlowAsync(accountService, characterService);
    }
    
    private static void RegisterEventHandlers(InMemoryGameEventPublisher eventPublisher)
    {
        // Manipuladores de eventos para Account
        eventPublisher.Subscribe<AccountCreatedEvent>(async e => {
            System.Console.WriteLine($"Evento: Conta criada - ID: {e.AccountId}, Username: {e.Username}");
            await Task.CompletedTask;
        });
        
        eventPublisher.Subscribe<AccountPasswordChangedEvent>(async e => {
            System.Console.WriteLine($"Evento: Senha alterada - ID: {e.AccountId}, Username: {e.Username}");
            await Task.CompletedTask;
        });
        
        eventPublisher.Subscribe<AccountStateChangedEvent>(async e => {
            System.Console.WriteLine($"Evento: Estado da conta alterado - ID: {e.AccountId}, Username: {e.Username}, Novo Estado: {e.NewState}");
            await Task.CompletedTask;
        });

        eventPublisher.Subscribe<AccountBannedEvent>(async e => {
            System.Console.WriteLine($"Evento: Conta banida - ID: {e.AccountId}, Username: {e.Username}, Motivo: {e.Reason}");
            await Task.CompletedTask;
        });

        eventPublisher.Subscribe<AccountSuspendedEvent>(async e => {
            System.Console.WriteLine($"Evento: Conta suspensa - ID: {e.AccountId}, Username: {e.Username}, Duração: {e.Duration}, Motivo: {e.Reason}");
            await Task.CompletedTask;
        });

        eventPublisher.Subscribe<AccountDeletedEvent>(async e => {
            System.Console.WriteLine($"Evento: Conta excluída - ID: {e.AccountId}, Username: {e.Username}");
            await Task.CompletedTask;
        });

        eventPublisher.Subscribe<InvalidAccountStateTransitionEvent>(async e => {
            System.Console.WriteLine($"Evento: Transição inválida - ID: {e.AccountId}, Username: {e.Username}, De: {e.CurrentState}, Para: {e.AttemptedState}, Motivo: {e.Reason}");
            await Task.CompletedTask;
        });
        
        // Manipuladores de eventos para Character
        eventPublisher.Subscribe<CharacterCreatedEvent>(async e => {
            System.Console.WriteLine($"Evento: Personagem criado - ID: {e.CharacterId}, Nome: {e.Name}");
            await Task.CompletedTask;
        });
        
        eventPublisher.Subscribe<CharacterDiedEvent>(async e => {
            System.Console.WriteLine($"Evento: Personagem morreu - ID: {e.CharacterId}, Nome: {e.Name}");
            await Task.CompletedTask;
        });
        
        eventPublisher.Subscribe<CharacterAddedToAccountEvent>(async e => {
            System.Console.WriteLine($"Evento: Personagem adicionado à conta - Conta: {e.Username}, Personagem: {e.CharacterName}");
            await Task.CompletedTask;
        });
    }
    
    private static async Task ValidateAccountStateMachineAsync(AccountService accountService, InMemoryAccountRepository accountRepository)
    {
        System.Console.WriteLine("\n=== Validação da Máquina de Estados de Contas ===\n");
        
        // Limpar as contas da memória para garantir consistência no teste
        System.Console.WriteLine("Preparando o ambiente de teste...");
        
        try
        {
            // PARTE 1: Testar transições a partir do estado CREATED
            System.Console.WriteLine("\n1. Testando transições a partir do estado Created:\n");
            
            // Created -> Activated
            System.Console.WriteLine("1.1 Transição Created -> Activated:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_created_activated", 
                null, // Sem estado inicial, cria nova conta
                AccountState.Activated,
                expectSuccess: true
            );
            
            // Created -> Locked
            System.Console.WriteLine("\n1.2 Transição Created -> Locked:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_created_locked", 
                null, // Sem estado inicial, cria nova conta
                AccountState.Locked,
                expectSuccess: true
            );
            
            // Created -> Suspended
            System.Console.WriteLine("\n1.3 Transição Created -> Suspended:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_created_suspended", 
                null, // Sem estado inicial, cria nova conta
                AccountState.Suspended,
                expectSuccess: true
            );
            
            // Created -> Banned
            System.Console.WriteLine("\n1.4 Transição Created -> Banned:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_created_banned", 
                null, // Sem estado inicial, cria nova conta
                AccountState.Banned,
                expectSuccess: true
            );
            
            // Created -> Deleted
            System.Console.WriteLine("\n1.5 Transição Created -> Deleted:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_created_deleted", 
                null, // Sem estado inicial, cria nova conta
                AccountState.Deleted,
                expectSuccess: true
            );
            
            // PARTE 2: Testar transições a partir do estado ACTIVATED
            System.Console.WriteLine("\n2. Testando transições a partir do estado Activated:\n");
            
            // Activated -> Locked
            System.Console.WriteLine("2.1 Transição Activated -> Locked:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_activated_locked", 
                AccountState.Activated, // Estado inicial
                AccountState.Locked,
                expectSuccess: true
            );
            
            // Activated -> Suspended
            System.Console.WriteLine("\n2.2 Transição Activated -> Suspended:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_activated_suspended", 
                AccountState.Activated, // Estado inicial
                AccountState.Suspended,
                expectSuccess: true
            );
            
            // Activated -> Banned
            System.Console.WriteLine("\n2.3 Transição Activated -> Banned:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_activated_banned", 
                AccountState.Activated, // Estado inicial
                AccountState.Banned,
                expectSuccess: true
            );
            
            // 9. Activated -> Deleted (válida)
            System.Console.WriteLine("\n2.4 Transição Activated -> Deleted:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_activated_deleted", 
                AccountState.Activated, // Estado inicial
                AccountState.Deleted,
                expectSuccess: true
            );
            
            // PARTE 3: Testar transições a partir de outros estados
            System.Console.WriteLine("\n3. Testando transições a partir de outros estados:\n");
            
            // Banned -> Activated
            System.Console.WriteLine("3.1 Transição Banned -> Activated:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_banned_activated", 
                AccountState.Banned, // Estado inicial
                AccountState.Activated,
                expectSuccess: true
            );
            
            // Banned -> Deleted
            System.Console.WriteLine("\n3.2 Transição Banned -> Deleted:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_banned_deleted", 
                AccountState.Banned, // Estado inicial
                AccountState.Deleted,
                expectSuccess: true
            );
            
            // Locked -> Activated
            System.Console.WriteLine("\n3.3 Transição Locked -> Activated:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_locked_activated", 
                AccountState.Locked, // Estado inicial
                AccountState.Activated,
                expectSuccess: true
            );
            
            // Suspended -> Activated
            System.Console.WriteLine("\n3.4 Transição Suspended -> Activated:");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_suspended_activated", 
                AccountState.Suspended, // Estado inicial
                AccountState.Activated,
                expectSuccess: true
            );
            
            // PARTE 4: Testar transições inválidas
            System.Console.WriteLine("\n4. Testando transições inválidas:\n");
            
            // Deleted -> Activated (inválida - estado terminal)
            System.Console.WriteLine("4.1 Transição Deleted -> Activated (deve falhar):");
            await TestAccountStateTransition(
                accountService, 
                accountRepository,
                "test_deleted_activated", 
                AccountState.Deleted, // Estado inicial
                AccountState.Activated,
                expectSuccess: false
            );
            
            System.Console.WriteLine("\n=== Validação da Máquina de Estados Concluída ===");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nERRO durante a validação: {ex.Message}\n");
            System.Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private static async Task TestAccountStateTransition(
        AccountService accountService,
        InMemoryAccountRepository accountRepository,
        string username,
        AccountState? initialState,
        AccountState targetState,
        bool expectSuccess)
    {
        // Criar nova conta para o teste
        await accountService.CreateAccountAsync(username, "password123");
        System.Console.WriteLine($"Conta '{username}' criada com sucesso no estado Created");
        
        var account = await accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            System.Console.WriteLine($"ERRO: Não foi possível encontrar a conta '{username}' após a criação");
            return;
        }
        
        // Configurar estado inicial se necessário
        if (initialState.HasValue && initialState.Value != AccountState.Created)
        {
            System.Console.WriteLine($"Configurando estado inicial para {initialState.Value}...");
            
            // Configuramos o estado inicial baseado no estado solicitado
            switch (initialState.Value)
            {
                case AccountState.Activated:
                    await accountService.ActivateAccountAsync(username);
                    break;
                case AccountState.Banned:
                    await accountService.ActivateAccountAsync(username); // Ativa primeiro
                    await accountService.BanAccountAsync(username, "Teste de banimento");
                    break;
                case AccountState.Locked:
                    await accountService.ActivateAccountAsync(username); // Ativa primeiro
                    await accountService.LockAccountAsync(username);
                    break;
                case AccountState.Suspended:
                    await accountService.ActivateAccountAsync(username); // Ativa primeiro
                    await accountService.SuspendAccountAsync(username, TimeSpan.FromDays(1), "Teste de suspensão");
                    break;
                case AccountState.Deleted:
                    await accountService.ActivateAccountAsync(username); // Ativa primeiro
                    await accountService.DeleteAccountAsync(username);
                    break;
            }
            
            // Verificar se o estado foi configurado corretamente
            account = await accountRepository.GetByUsernameAsync(username);
            if (account == null)
            {
                System.Console.WriteLine($"ERRO: Não foi possível encontrar a conta '{username}' após configuração de estado");
                return;
            }
            
            if (account.State != initialState.Value)
            {
                System.Console.WriteLine($"AVISO: Falha ao configurar estado inicial. Esperava {initialState.Value}, mas o estado atual é {account.State}");
            }
            else
            {
                System.Console.WriteLine($"Estado inicial configurado com sucesso: {account.State}");
            }
        }
        
        // Recarregar a conta para ver o estado atual antes da transição
        account = await accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            System.Console.WriteLine($"ERRO: Não foi possível encontrar a conta '{username}' antes da transição");
            return;
        }
        
        System.Console.WriteLine($"Estado atual da conta '{username}': {account.State}");
        System.Console.WriteLine($"Tentando transição para {targetState}...");
        
        // Realizar a transição
        bool success = false;
        try
        {
            switch (targetState)
            {
                case AccountState.Activated:
                    success = await accountService.ActivateAccountAsync(username);
                    break;
                case AccountState.Banned:
                    success = await accountService.BanAccountAsync(username, "Teste de banimento");
                    break;
                case AccountState.Locked:
                    success = await accountService.LockAccountAsync(username);
                    break;
                case AccountState.Suspended:
                    success = await accountService.SuspendAccountAsync(username, TimeSpan.FromDays(1), "Teste de suspensão");
                    break;
                case AccountState.Deleted:
                    success = await accountService.DeleteAccountAsync(username);
                    break;
                default:
                    System.Console.WriteLine($"ERRO: Estado alvo não suportado: {targetState}");
                    return;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"EXCEÇÃO durante a transição: {ex.Message}");
            success = false;
        }
        
        // Recarregar a conta para ver o estado atual após a tentativa de transição
        account = await accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            System.Console.WriteLine($"AVISO: Não foi possível encontrar a conta '{username}' após a transição. Isso pode ser normal se a conta foi excluída.");
            
            // Se esperamos que a transição seja para deleted, isso é normal
            if (targetState == AccountState.Deleted && expectSuccess)
            {
                System.Console.WriteLine("A conta foi excluída conforme esperado.");
                return;
            }
            
            return;
        }
        
        System.Console.WriteLine($"Resultado da transição: {(success ? "Sucesso" : "Falha")}");
        System.Console.WriteLine($"Estado final da conta: {account.State}");
        
        // Validar o resultado
        if (success != expectSuccess)
        {
            System.Console.WriteLine($"ERRO: Esperava que a transição {(expectSuccess ? "tivesse sucesso" : "falhasse")}, mas ela {(success ? "teve sucesso" : "falhou")}");
        }
        else if (expectSuccess && account.State != targetState)
        {
            System.Console.WriteLine($"ERRO: Esperava que o estado final fosse {targetState}, mas é {account.State}");
        }
        else if (!expectSuccess && account.State == targetState)
        {
            System.Console.WriteLine($"ERRO: A transição não deveria ter ocorrido, mas o estado mudou para {targetState}");
        }
        else
        {
            System.Console.WriteLine($"Validação da transição: OK");
        }
    }
    
    private static async Task DemoGameFlowAsync(AccountService accountService, CharacterService characterService)
    {
        try
        {
            System.Console.WriteLine("=== Demonstração do Servidor de Jogo com Arquitetura Hexagonal ===\n");
            
            // 1. Criar uma conta
            System.Console.WriteLine("1. Criando conta de usuário...");
            await accountService.CreateAccountAsync("jogador1", "senha123");
            System.Console.WriteLine("Conta criada com sucesso!\n");
            
            // 2. Autenticar
            System.Console.WriteLine("2. Autenticando usuário...");
            bool isAuthenticated = await accountService.AuthenticateAsync("jogador1", "senha123");
            System.Console.WriteLine($"Autenticação: {(isAuthenticated ? "Sucesso" : "Falha")}\n");
            
            // 3. Ativar a conta
            System.Console.WriteLine("3. Ativando conta...");
            await accountService.ActivateAccountAsync("jogador1");
            System.Console.WriteLine("Conta ativada!\n");
            
            // 4. Criar um personagem
            System.Console.WriteLine("4. Criando personagem...");
            var stats = new Stats { Strength = 10, Defense = 8, Agility = 12 };
            var vital = new Vital { Health = 100, MaxHealth = 100, Mana = 50, MaxMana = 50 };
            var boundingBox = new BoundingBox(100, 100, 32, 32);
            
            var character = await characterService.CreateCharacterAsync(
                "Herói", 
                stats, 
                vital, 
                boundingBox, 
                Direction.Down, 
                1);
                
            System.Console.WriteLine($"Personagem '{character.Name}' criado com ID: {character.Id}\n");
            
            // 5. Adicionar personagem à conta
            System.Console.WriteLine("5. Adicionando personagem à conta...");
            await accountService.AddCharacterAsync("jogador1", character);
            System.Console.WriteLine("Personagem adicionado à conta!\n");
            
            // 6. Mover o personagem
            System.Console.WriteLine("6. Movendo o personagem...");
            await characterService.MoveCharacterAsync(character.Id, Direction.Right);
            character = await characterService.GetCharacterByIdAsync(character.Id);
            System.Console.WriteLine($"Nova posição: X={character.BoundingBox.X}, Y={character.BoundingBox.Y}, Direção={character.Direction}\n");
            
            // 7. Ataque simulado
            System.Console.WriteLine("7. Simulando dano ao personagem...");
            await characterService.TakeDamageAsync(character.Id, 20);
            character = await characterService.GetCharacterByIdAsync(character.Id);
            System.Console.WriteLine($"Vida após dano: {character.Vital.Health}/{character.Vital.MaxHealth}\n");
            
            // 8. Curar o personagem
            System.Console.WriteLine("8. Curando o personagem...");
            await characterService.HealAsync(character.Id, 15);
            character = await characterService.GetCharacterByIdAsync(character.Id);
            System.Console.WriteLine($"Vida após cura: {character.Vital.Health}/{character.Vital.MaxHealth}\n");
            
            // 9. Matar o personagem
            System.Console.WriteLine("9. Matando o personagem...");
            await characterService.KillCharacterAsync(character.Id);
            character = await characterService.GetCharacterByIdAsync(character.Id);
            System.Console.WriteLine($"Estado do personagem: {character.State}, Vida: {character.Vital.Health}\n");
            
            // 10. Reviver o personagem
            System.Console.WriteLine("10. Revivendo o personagem...");
            await characterService.ReviveCharacterAsync(character.Id, 0.5);  // Revive com 50% da vida
            character = await characterService.GetCharacterByIdAsync(character.Id);
            System.Console.WriteLine($"Estado após reviver: {character.State}, Vida: {character.Vital.Health}/{character.Vital.MaxHealth}\n");
            
            System.Console.WriteLine("Demonstração concluída!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Erro durante a demonstração: {ex.Message}");
        }
    }
}