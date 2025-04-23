# Máquina de Estados para Personagens (Character State Machine)

## Visão Geral
Este documento descreve a implementação da máquina de estados que controla as transições de estado para personagens no sistema. A máquina de estados garante que apenas transições válidas entre estados possam ocorrer, mantendo a integridade do domínio e permitindo reações apropriadas às mudanças de estado.

## Estados Disponíveis
O personagem pode estar em um dos seguintes estados:

- **Idle (0)**: Estado padrão quando o personagem não está realizando nenhuma ação específica
- **Walking (1)**: Personagem está em movimento
- **Attacking (2)**: Personagem está executando um ataque
- **Dead (3)**: Personagem morreu (estado terminal em alguns casos)

## Diagrama de Transições de Estado
```
                     ┌───────────┐
                     │           │
          ┌──────────► Idle      ◄──────────┐
          │          │           │          │
          │          └─────┬─────┘          │
          │                │                │
          │                ▼                │
          │          ┌───────────┐          │
          │          │           │          │
Transição │          │ Walking   │          │ Transição
Permitida │    ┌─────►           ◄─────┐    │ Permitida
          │    │     └─────┬─────┘     │    │
          │    │           │           │    │
          │    │           ▼           │    │
          │    │     ┌───────────┐     │    │
          │    │     │           │     │    │
          │    └─────┤ Attacking ├─────┘    │
          │          │           │          │
          │          └─────┬─────┘          │
          │                │                │
          │                ▼                │
          │          ┌───────────┐          │
          │          │           │          │
          └──────────┤ Dead      ├──────────┘
                     │           │
                     └───────────┘
```

## Transições Permitidas

| Estado Atual | Estados Possíveis           |
|--------------|----------------------------|
| Idle         | Walking, Attacking, Dead   |
| Walking      | Idle, Attacking, Dead      |
| Attacking    | Idle, Walking, Dead        |
| Dead         | Idle (em caso de ressurreição) |

## Implementação

### Classe `CharacterStateTransition`
A classe `CharacterStateTransition` na camada de domínio define as regras de transição de estado e fornece métodos para validar transições. Ela implementa:

- Um mapa de transições válidas para cada estado
- Método `IsValidTransition` para validar se uma transição é permitida
- Método `GetPossibleTransitions` para obter todos os estados possíveis a partir de um estado atual

### Métodos de Transição na Entidade `Character`
A entidade `Character` implementa os seguintes métodos para gerenciar as transições de estado:

- `TryChangeState`: Método interno que centraliza a lógica de validação de transição
- `StartWalking`: Tenta colocar o personagem no estado de movimento
- `StopWalking`: Tenta interromper o movimento, voltando para Idle
- `StartAttacking`: Tenta iniciar um ataque
- `StopAttacking`: Tenta finalizar um ataque, voltando para Idle ou Walking
- `Die`: Coloca o personagem no estado Dead
- `Resurrect`: Permite que um personagem morto volte ao estado Idle (quando aplicável)
- `GetPossibleStateTransitions`: Retorna as transições possíveis para o estado atual

Cada método de transição retorna um valor booleano indicando se a transição foi bem-sucedida.

### Eventos de Domínio
Quando uma transição de estado ocorre, os seguintes eventos de domínio são disparados:

- `CharacterStateChangedEvent`: Evento base para notificar sobre qualquer mudança de estado
- `CharacterStartedWalkingEvent`: Notifica sobre o início de movimento, incluindo direção e posição
- `CharacterStartedAttackingEvent`: Notifica sobre o início de um ataque, incluindo alvo e dano potencial
- `CharacterDiedEvent`: Notifica sobre a morte do personagem, incluindo causa e responsável
- `InvalidCharacterStateTransitionEvent`: Notifica sobre uma tentativa de transição inválida

## Benefícios

- **Coordenação de animações**: Facilita a sincronização entre estado lógico e representação visual
- **Controle de colisões**: Permite comportamentos diferentes de física baseados no estado
- **Regras de combate claras**: Define quais ações são permitidas durante o combate
- **Auditoria de jogabilidade**: Registra as ações do personagem através de eventos de domínio
- **Prevenção de bugs**: Evita transições inválidas que poderiam causar comportamentos inesperados
- **Flexibilidade de design**: Facilita a adição de novos estados e comportamentos