# Sistema de Eventos de Transição de Estado

## Visão Geral

Este documento descreve o sistema de eventos de domínio implementado para capturar transições de estado nas máquinas de estados de entidades do domínio. O sistema fornece uma estrutura genérica e extensível para registrar, rastrear e reagir a mudanças de estado em diferentes entidades.

## Arquitetura de Eventos de Estado

### Classe Base `DomainEvent`

Todos os eventos de domínio herdam da classe abstrata `DomainEvent`, que fornece:
- Um identificador único (`EventId`) para cada evento
- Um timestamp (`OccurredOn`) registrando quando o evento ocorreu

### Eventos Genéricos de Mudança de Estado

#### `StateChangedEvent<TEnum>`

Esta classe base genérica serve como fundação para todos os eventos de mudança de estado:

```csharp
public abstract class StateChangedEvent<TEnum> : DomainEvent where TEnum : System.Enum
{
    public TEnum PreviousState { get; }
    public TEnum NewState { get; }
    public long EntityId { get; }
    public DateTime TransitionTime { get; }
    public string? Reason { get; }
    
    // ...construtor e métodos
}
```

- **Propósito**: Capturar qualquer transição de um estado para outro em qualquer entidade que use enums para representar estados
- **Aplicabilidade**: Pode ser usado com qualquer tipo de enum, tornando-o flexível para diferentes tipos de entidades

#### `InvalidStateTransitionEvent<TEnum>`

Esta classe base genérica captura tentativas de transição inválidas:

```csharp
public class InvalidStateTransitionEvent<TEnum> : DomainEvent where TEnum : System.Enum
{
    public TEnum CurrentState { get; }
    public TEnum AttemptedState { get; }
    public long EntityId { get; }
    public string ErrorMessage { get; }
    
    // ...construtor e métodos
}
```

- **Propósito**: Registrar quando uma entidade tenta realizar uma transição de estado não permitida
- **Benefícios**: Facilita auditoria e ajuda na depuração de problemas relacionados à máquina de estados

### Especializações por Tipo de Entidade

#### Para Contas (`Account`)

- `AccountStateChangedEvent`: Especialização para transições de estado de contas
- `InvalidAccountStateTransitionEvent`: Especialização para transições inválidas em contas

```csharp
// Exemplos de eventos específicos para Account
public class AccountBannedEvent : AccountStateChangedEvent { /* ... */ }
public class AccountSuspendedEvent : AccountStateChangedEvent { /* ... */ }
public class AccountDeletedEvent : AccountStateChangedEvent { /* ... */ }
```

#### Para Personagens (`Character`)

- `CharacterStateChangedEvent`: Especialização para transições de estado de personagens
- `InvalidCharacterStateTransitionEvent`: Especialização para transições inválidas em personagens

```csharp
// Exemplos de eventos específicos para Character
public class CharacterDiedEvent : CharacterStateChangedEvent { /* ... */ }
public class CharacterStartedWalkingEvent : CharacterStateChangedEvent { /* ... */ }
public class CharacterStartedAttackingEvent : CharacterStateChangedEvent { /* ... */ }
```

## Fluxo de Trabalho dos Eventos de Estado

1. **Geração do Evento**:
   - Uma entidade tenta mudar seu estado usando um método de transição
   - Se a transição for válida, um evento `*StateChangedEvent` é criado e adicionado à lista de eventos da entidade
   - Se a transição for inválida, um evento `Invalid*StateTransitionEvent` é gerado

2. **Propagação do Evento**:
   - A camada de aplicação coleta os eventos pendentes da entidade após cada operação
   - Os eventos são então dispersados para handlers apropriados para processamento

3. **Processamento do Evento**:
   - Handlers específicos para cada tipo de evento realizam ações apropriadas
   - Podem incluir: logging, notificações, atualizações em serviços externos, etc.

## Benefícios do Sistema de Eventos de Estado

1. **Desacoplamento**: A lógica de transição de estado está separada das consequências da transição
2. **Auditabilidade**: Cada mudança de estado é registrada com detalhes relevantes
3. **Extensibilidade**: Novas reações às mudanças de estado podem ser adicionadas sem modificar o modelo de domínio
4. **Consistência**: Abordagem uniforme para lidar com transições de estado em diferentes entidades
5. **Rastreabilidade**: Facilita a depuração e análise do comportamento do sistema
6. **Reusabilidade**: A estrutura genérica permite aplicar o mesmo padrão a novas entidades com máquinas de estado

## Exemplo de Uso na Entidade

```csharp
// Exemplo de método para mudar estado em uma entidade Character
public bool TryChangeState(CharacterState newState, string? reason = null)
{
    // Verificar se a transição é válida
    if (!CharacterStateTransition.IsValidTransition(CurrentState, newState))
    {
        // Gerar evento de transição inválida
        AddDomainEvent(new InvalidCharacterStateTransitionEvent(
            CurrentState, 
            newState, 
            Id, 
            $"Transição inválida de {CurrentState} para {newState}"));
        return false;
    }
    
    // Realizar transição
    var previousState = CurrentState;
    CurrentState = newState;
    
    // Gerar evento apropriado com base no novo estado
    switch (newState)
    {
        case CharacterState.Dead:
            AddDomainEvent(new CharacterDiedEvent(previousState, Id, reason ?? "Morte não especificada"));
            break;
        // ... outros casos específicos
        default:
            AddDomainEvent(new CharacterStateChangedEvent(previousState, newState, Id, reason));
            break;
    }
    
    return true;
}
```