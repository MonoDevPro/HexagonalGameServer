# Máquina de Estados para Conta (Account State Machine)

## Visão Geral
Este documento descreve a implementação da máquina de estados que controla as transições de estado para contas de usuário no sistema. A máquina de estados garante que apenas transições válidas entre estados possam ocorrer, mantendo a integridade do domínio.

## Estados Disponíveis
A conta de usuário pode estar em um dos seguintes estados:

- **None (0)**: Estado inicial/nulo, não utilizado em contas ativas
- **Created (1)**: Conta recém-criada, ainda não ativada
- **Activated (2)**: Conta ativa e em pleno funcionamento
- **Banned (3)**: Conta banida por violação de regras
- **Deleted (4)**: Conta excluída (estado terminal)
- **Locked (5)**: Conta temporariamente bloqueada
- **Suspended (6)**: Conta suspensa por um período determinado

## Diagrama de Transições de Estado
```
                   ┌───────────┐
                   │           │
         ┌─────────► Created   ├─────────┐
         │         │           │         │
         │         └─────┬─────┘         │
         │               │               │
         │               ▼               │
         │         ┌───────────┐         │
         │         │           │         │
Transição│  ┌──────► Activated ◄───────┐ │Transição
Permitida│  │      │           │       │ │Permitida
         │  │      └─┬───────┬─┘       │ │
         │  │        │       │         │ │
         │  │        ▼       ▼         │ │
         │  │  ┌─────────┐ ┌─────────┐ │ │
         │  │  │         │ │         │ │ │
         │  └──┤ Locked  │ │Suspended├─┘ │
         │     │         │ │         │   │
         │     └────┬────┘ └────┬────┘   │
         │          │           │        │
         │          ▼           ▼        │
         │        ┌───────────┐          │
         │        │           │          │
         └─────────► Banned   ├──────────┘
                  │           │
                  └─────┬─────┘
                        │
                        ▼
                  ┌───────────┐
                  │           │
                  │ Deleted   │ (Estado Terminal)
                  │           │
                  └───────────┘
```

## Transições Permitidas

| Estado Atual | Estados Possíveis                                  |
|--------------|---------------------------------------------------|
| Created      | Activated, Locked, Suspended, Banned, Deleted      |
| Activated    | Locked, Suspended, Banned, Deleted                 |
| Locked       | Activated, Suspended, Banned, Deleted              |
| Suspended    | Activated, Locked, Banned, Deleted                 |
| Banned       | Activated, Deleted                                 |
| Deleted      | Nenhum (estado terminal)                           |

## Implementação

### Classe `AccountStateTransition`
A classe `AccountStateTransition` na camada de domínio define as regras de transição de estado e fornece métodos para validar transições. Ela implementa:

- Um mapa de transições válidas para cada estado
- Método `IsValidTransition` para validar se uma transição é permitida
- Método `GetPossibleTransitions` para obter todos os estados possíveis a partir de um estado atual

### Métodos de Transição na Entidade `Account`
A entidade `Account` implementa os seguintes métodos para gerenciar as transições de estado:

- `TryChangeState`: Método interno que centraliza a lógica de validação de transição
- `Activate`: Tenta ativar a conta
- `Ban`: Tenta banir a conta
- `Lock`: Tenta bloquear a conta
- `Suspend`: Tenta suspender a conta
- `Delete`: Tenta excluir a conta
- `GetPossibleStateTransitions`: Retorna as transições possíveis para o estado atual

Cada método de transição retorna um valor booleano indicando se a transição foi bem-sucedida.

### Eventos de Domínio
Quando uma transição de estado ocorre, os seguintes eventos de domínio podem ser disparados:

- `AccountStateChangedEvent`: Notifica sobre qualquer mudança de estado
- `AccountBannedEvent`: Notifica especificamente sobre um banimento
- `AccountSuspendedEvent`: Notifica sobre uma suspensão, incluindo duração
- `AccountDeletedEvent`: Notifica sobre a exclusão da conta
- `InvalidAccountStateTransitionEvent`: Notifica sobre uma tentativa de transição inválida

## Utilização na Camada de Aplicação
O `AccountService` na camada de aplicação expõe métodos que encapsulam as transições de estado:

- `ActivateAccountAsync`
- `BanAccountAsync`
- `LockAccountAsync`
- `SuspendAccountAsync`
- `DeleteAccountAsync`
- `GetPossibleStateTransitionsAsync`
- `GetAccountStateAsync`

Estes métodos retornam um valor booleano indicando o sucesso da operação.

## Benefícios

- **Integridade do domínio**: Garante que apenas transições válidas ocorram
- **Encapsulamento**: Centraliza regras de transição em um único lugar
- **Auditoria**: Registra tentativas inválidas através de eventos de domínio
- **Flexibilidade**: Facilita a adição de novos estados ou regras de transição
- **Transparência**: Torna explícito quais transições são possíveis em cada momento