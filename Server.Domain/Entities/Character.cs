using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Primitives;

// Add Primitives namespace

namespace Server.Domain.Entities;

public class Character : Entity
{
    // Propriedades de domínio, com ‘setters’ protegidos para garantir invariantes
    public string Name { get; private set; }
    public Stats Stats { get; private set; }
    public Vital Vital { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Direction Direction { get; private set; }
    public CharacterState State { get; private set; }
    public int FloorIndex { get; private set; }

    // Construtor vazio para EF Core ou outros ORMs
    protected Character() { }

    // Construtor público para criação de domínio,
    // onde você valida invariantes e adiciona DomainEvents
    public Character(string name,
        Stats stats, // Struct
        Vital vital, // Struct
        BoundingBox boundingBox, // Struct
        Direction direction,
        CharacterState initialState,
        int floorIndex)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Stats = stats;
        Vital = vital;
        BoundingBox = boundingBox;
        Direction = direction;
        State = initialState;
        FloorIndex = floorIndex;

        // Exemplo de DomainEvent levantado no Aggregate
        AddDomainEvent(new CharacterCreatedEvent(Id, Name));
    }

    // Métodos de comportamento do Character, que explicitam regras de negócio
    
    // Movimento e posicionamento
    public void Move(Direction direction)
    {
        // Define a nova direção para o personagem
        Direction = direction;
        
        // Somente faz o movimento se o personagem não estiver morto
        if (State == CharacterState.Dead)
            return;
            
        // Calcula o deslocamento baseado na direção
        Vector2 movement = GetMovementVector(direction);
        
        // Guarda o estado anterior para o evento
        CharacterState previousState = State;
        
        // Atualiza a posição da BoundingBox
        BoundingBox = new BoundingBox(
            BoundingBox.X + movement.X,
            BoundingBox.Y + movement.Y,
            BoundingBox.Width,
            BoundingBox.Height);
            
        // Atualiza o estado para Walking
        State = CharacterState.Walking;
        
        // Emite o evento de início de movimento
        if (previousState != CharacterState.Walking)
        {
            AddDomainEvent(new CharacterStartedWalkingEvent(
                previousState,
                Id,
                direction.ToString(),
                BoundingBox.X - movement.X, // Posição inicial X
                BoundingBox.Y - movement.Y, // Posição inicial Y
                1.0f // Velocidade padrão
            ));
        }
    }
    
    public void StopMoving()
    {
        if (State == CharacterState.Walking)
        {
            CharacterState previousState = State;
            State = CharacterState.Idle;
            
            // Emite o evento de mudança de estado
            AddDomainEvent(new CharacterStateChangedEvent(
                previousState,
                State,
                Id,
                "Parou de se mover"
            ));
        }
    }
    
    public void ChangeFloor(int newFloorIndex)
    {
        if (newFloorIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(newFloorIndex), "Floor index cannot be negative");
            
        FloorIndex = newFloorIndex;
    }
    
    // Combate
    public void Attack(Character target)
    {
        if (State == CharacterState.Dead)
            return;
            
        // Verificar se o alvo está no alcance
        if (IsInAttackRange(target))
        {
            CharacterState previousState = State;
            State = CharacterState.Attacking;
            
            // Calcular dano baseado nos stats
            double damage = CalculateDamage(Stats.Strength, target.Stats.Defense);
            
            // Emitir evento de ataque
            AddDomainEvent(new CharacterStartedAttackingEvent(
                previousState,
                Id,
                "Ataque básico",
                (int)damage,
                BoundingBox.X,
                BoundingBox.Y,
                target.Id
            ));
            
            // Aplicar dano ao alvo
            target.TakeDamage(damage);
        }
    }
    
    public void TakeDamage(double amount)
    {
        if (State == CharacterState.Dead)
            return;
            
        // Não permitir valores negativos de dano
        if (amount < 0)
            amount = 0;
            
        // Reduzir vida do personagem
        Vital = new Vital
        {
            Health = Math.Max(0, Vital.Health - amount),
            MaxHealth = Vital.MaxHealth,
            Mana = Vital.Mana,
            MaxMana = Vital.MaxMana
        };
        
        // Verificar se o personagem morreu
        if (Vital.Health <= 0)
        {
            Die();
        }
    }
    
    public void Heal(double amount)
    {
        if (State == CharacterState.Dead)
            return;
            
        // Não permitir valores negativos de cura
        if (amount < 0)
            amount = 0;
            
        // Aumentar vida do personagem sem ultrapassar o máximo
        Vital = new Vital
        {
            Health = Math.Min(Vital.MaxHealth, Vital.Health + amount),
            MaxHealth = Vital.MaxHealth,
            Mana = Vital.Mana,
            MaxMana = Vital.MaxMana
        };
    }
    
    public void UseMana(double amount)
    {
        if (State == CharacterState.Dead || amount <= 0)
            return;
            
        // Verificar se tem mana suficiente
        if (Vital.Mana >= amount)
        {
            Vital = new Vital
            {
                Health = Vital.Health,
                MaxHealth = Vital.MaxHealth,
                Mana = Vital.Mana - amount,
                MaxMana = Vital.MaxMana
            };
        }
    }
    
    public void RestoreMana(double amount)
    {
        if (State == CharacterState.Dead)
            return;
            
        // Não permitir valores negativos de restauração
        if (amount < 0)
            amount = 0;
            
        // Aumentar mana do personagem sem ultrapassar o máximo
        Vital = new Vital
        {
            Health = Vital.Health,
            MaxHealth = Vital.MaxHealth,
            Mana = Math.Min(Vital.MaxMana, Vital.Mana + amount),
            MaxMana = Vital.MaxMana
        };
    }
    
    public void Die()
    {
        if (State != CharacterState.Dead)
        {
            CharacterState previousState = State;
            State = CharacterState.Dead;
            
            // Zerar a vida para garantir que está morto
            Vital = new Vital
            {
                Health = 0,
                MaxHealth = Vital.MaxHealth,
                Mana = Vital.Mana,
                MaxMana = Vital.MaxMana
            };
            
            // Adicionar evento de domínio para notificar a morte
            AddDomainEvent(new CharacterDiedEvent(
                previousState,
                Id,
                Name,
                "Morte em combate", // Causa da morte 
                null,               // ID do assassino (opcional)
                $"Posição: {BoundingBox.X},{BoundingBox.Y}" // Localização
            ));
        }
    }
    
    public void Revive(double healthPercentage = 0.1)
    {
        if (State == CharacterState.Dead)
        {
            // Validar porcentagem entre 0.01 e 1.0
            healthPercentage = Math.Clamp(healthPercentage, 0.01, 1.0);
            
            // Guardar o estado anterior para o evento
            CharacterState previousState = State;
            
            // Restaurar uma porcentagem da vida máxima
            Vital = new Vital
            {
                Health = Vital.MaxHealth * healthPercentage,
                MaxHealth = Vital.MaxHealth,
                Mana = Vital.Mana,
                MaxMana = Vital.MaxMana
            };
            
            State = CharacterState.Idle;
            
            // Emitir evento de mudança de estado
            AddDomainEvent(new CharacterStateChangedEvent(
                previousState,
                State,
                Id,
                $"Ressuscitado com {healthPercentage * 100}% de vida"
            ));
        }
    }
    
    // Stats e progressão
    public void IncreaseStats(Stats statsIncrease)
    {
        Stats = new Stats
        {
            Strength = Stats.Strength + statsIncrease.Strength,
            Defense = Stats.Defense + statsIncrease.Defense,
            Agility = Stats.Agility + statsIncrease.Agility
        };
    }
    
    // Métodos auxiliares (privados)
    private Vector2 GetMovementVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2(0, -1),
            Direction.Down => new Vector2(0, 1),
            Direction.Left => new Vector2(-1, 0),
            Direction.Right => new Vector2(1, 0),
            Direction.UpLeft => new Vector2(-1, -1),
            Direction.UpRight => new Vector2(1, -1),
            Direction.DownLeft => new Vector2(-1, 1),
            Direction.DownRight => new Vector2(1, 1),
            _ => Vector2.Zero
        };
    }
    
    private bool IsInAttackRange(Character target)
    {
        // Implementação simples de verificação de distância
        var distance = BoundingBox.Location.DistanceTo(target.BoundingBox.Location);
        
        // Distância máxima para atacar - pode ser ajustada ou baseada em algum atributo
        const int MaxAttackDistance = 10;
        
        return distance <= MaxAttackDistance && FloorIndex == target.FloorIndex;
    }
    
    private double CalculateDamage(double attackerStrength, double defenderDefense)
    {
        // Fórmula simples para cálculo de dano
        double baseDamage = attackerStrength * 2.0;
        double damage = Math.Max(1, baseDamage - defenderDefense);
        
        // Adicionar um elemento de aleatoriedade (±20%)
        Random random = new Random();
        double randomFactor = 0.8 + (random.NextDouble() * 0.4); // Entre 0.8 e 1.2
        
        return Math.Round(damage * randomFactor);
    }
}
