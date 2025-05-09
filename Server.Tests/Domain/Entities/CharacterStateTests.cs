using System;
using Server.Domain.Entities;
using Server.Domain.Entities.Primitives;
using Server.Domain.Enum;
using Server.Domain.Events;
using Server.Domain.Events.Character;
using Server.Domain.ValueObjects;
using Xunit;

namespace Server.Tests.Domain.Entities;

public class CharacterStateTests
{
    private Character _character;
    
    public CharacterStateTests()
    {
        // Criar um personagem para os testes com valores iniciais
        _character = new Character(
            name: "TestCharacter",
            stats: new Stats { Strength = 10, Defense = 5, Agility = 8 },
            vital: new Vital { Health = 100, MaxHealth = 100, Mana = 50, MaxMana = 50 },
            boundingBox: new BoundingBox(0, 0, 1, 1),
            direction: Direction.Down,
            initialState: CharacterState.Idle,
            floorIndex: 1
        );
    }
    
    [Fact]
    public void NewCharacter_HasIdleState()
    {
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State);
        
        // Verificar se o evento CharacterCreatedEvent foi gerado
        var events = _character.GetDomainEvents();
        Assert.Contains(events, e => e is CharacterCreatedEvent);
    }
    
    [Fact]
    public void Move_FromIdleState_ChangesToWalkingState()
    {
        // Arrange
        _character.ClearDomainEvents(); // Limpar eventos existentes
        
        // Act
        _character.Move(Direction.Right);
        
        // Assert
        Assert.Equal(CharacterState.Walking, _character.State);
        Assert.Equal(Direction.Right, _character.Direction);
    }
    
    [Fact]
    public void StopMoving_FromWalkingState_ChangesToIdleState()
    {
        // Arrange
        _character.Move(Direction.Right); // Primeiro coloca o personagem no estado Walking
        _character.ClearDomainEvents(); // Limpar eventos existentes
        
        // Act
        _character.StopMoving();
        
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State);
    }
    
    [Fact]
    public void Attack_FromIdleState_ChangesToAttackingState()
    {
        // Arrange
        var target = new Character(
            name: "TargetCharacter",
            stats: new Stats { Strength = 8, Defense = 4, Agility = 7 },
            vital: new Vital { Health = 100, MaxHealth = 100, Mana = 50, MaxMana = 50 },
            boundingBox: new BoundingBox(5, 0, 1, 1), // Próximo o suficiente para atacar
            direction: Direction.Left,
            initialState: CharacterState.Idle,
            floorIndex: 1
        );
        _character.ClearDomainEvents();
        
        // Act
        _character.Attack(target);
        
        // Assert
        Assert.Equal(CharacterState.Attacking, _character.State);
    }
    
    [Fact]
    public void TakeDamage_WhenDamageExceedsHealth_ChangesToDeadState()
    {
        // Arrange
        _character.ClearDomainEvents();
        
        // Act
        _character.TakeDamage(200); // Dano maior que a vida atual
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State);
        Assert.Equal(0, _character.Vital.Health);
        
        // Verificar se o evento CharacterDiedEvent foi gerado
        var events = _character.GetDomainEvents();
        Assert.Contains(events, e => e is CharacterDiedEvent);
    }
    
    [Fact]
    public void TakeDamage_WhenHealthRemainsAboveZero_StateUnchanged()
    {
        // Arrange
        _character.ClearDomainEvents();
        
        // Act
        _character.TakeDamage(50); // Dano menor que a vida atual
        
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State); // Estado permanece Idle
        Assert.Equal(50, _character.Vital.Health); // Vida reduzida mas acima de zero
    }
    
    [Fact]
    public void Revive_FromDeadState_ChangesToIdleState()
    {
        // Arrange
        _character.Die(); // Primeiro coloca o personagem no estado Dead
        _character.ClearDomainEvents();
        
        // Act
        _character.Revive(0.5); // Revive com 50% da vida
        
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State);
        Assert.Equal(50, _character.Vital.Health); // 50% de 100
    }
    
    [Fact]
    public void Move_WhenInDeadState_StateRemainsDeadAndPositionUnchanged()
    {
        // Arrange
        var initialX = _character.BoundingBox.X;
        var initialY = _character.BoundingBox.Y;
        _character.Die(); // Coloca o personagem no estado Dead
        _character.ClearDomainEvents();
        
        // Act
        _character.Move(Direction.Right);
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State); // Estado permanece Dead
        Assert.Equal(initialX, _character.BoundingBox.X); // Posição X não muda
        Assert.Equal(initialY, _character.BoundingBox.Y); // Posição Y não muda
    }
    
    [Fact]
    public void Attack_WhenInDeadState_NoEffect()
    {
        // Arrange
        var target = new Character(
            name: "TargetCharacter",
            stats: new Stats { Strength = 8, Defense = 4, Agility = 7 },
            vital: new Vital { Health = 100, MaxHealth = 100, Mana = 50, MaxMana = 50 },
            boundingBox: new BoundingBox(5, 0, 1, 1),
            direction: Direction.Left,
            initialState: CharacterState.Idle,
            floorIndex: 1
        );
        var initialTargetHealth = target.Vital.Health;
        
        _character.Die(); // Coloca o personagem no estado Dead
        _character.ClearDomainEvents();
        
        // Act
        _character.Attack(target);
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State); // Estado permanece Dead
        Assert.Equal(initialTargetHealth, target.Vital.Health); // Vida do alvo não é afetada
    }
    
    [Fact]
    public void Heal_WhenInDeadState_NoEffect()
    {
        // Arrange
        _character.Die(); // Coloca o personagem no estado Dead
        _character.ClearDomainEvents();
        
        // Act
        _character.Heal(50);
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State); // Estado permanece Dead
        Assert.Equal(0, _character.Vital.Health); // Vida permanece zero
    }
    
    [Fact]
    public void UseMana_WhenInDeadState_NoEffect()
    {
        // Arrange
        var initialMana = 50.0;
        _character = new Character(
            name: "TestCharacter",
            stats: new Stats { Strength = 10, Defense = 5, Agility = 8 },
            vital: new Vital { Health = 100, MaxHealth = 100, Mana = initialMana, MaxMana = 50 },
            boundingBox: new BoundingBox(0, 0, 1, 1),
            direction: Direction.Down,
            initialState: CharacterState.Idle,
            floorIndex: 1
        );
        
        _character.Die(); // Coloca o personagem no estado Dead
        _character.ClearDomainEvents();
        
        // Act
        _character.UseMana(20);
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State); // Estado permanece Dead
        Assert.Equal(initialMana, _character.Vital.Mana); // Mana não é consumida
    }
    
    [Fact]
    public void StopMoving_WhenNotInWalkingState_NoChange()
    {
        // Arrange - personagem no estado Idle (não Walking)
        _character.ClearDomainEvents();
        
        // Act
        _character.StopMoving();
        
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State); // Estado permanece Idle
    }
    
    [Fact]
    public void Die_WhenAlreadyDead_NoAdditionalEvents()
    {
        // Arrange
        _character.Die(); // Primeiro coloca o personagem no estado Dead
        _character.ClearDomainEvents(); // Limpa eventos existentes
        
        // Act
        _character.Die(); // Tenta colocar no estado Dead novamente
        
        // Assert
        Assert.Equal(CharacterState.Dead, _character.State);
        Assert.Empty(_character.GetDomainEvents()); // Não deve gerar novos eventos
    }
    
    [Fact]
    public void Revive_WhenNotDead_NoEffect()
    {
        // Arrange - personagem no estado Idle (não Dead)
        var initialHealth = _character.Vital.Health;
        _character.ClearDomainEvents();
        
        // Act
        _character.Revive(0.5);
        
        // Assert
        Assert.Equal(CharacterState.Idle, _character.State); // Estado permanece Idle
        Assert.Equal(initialHealth, _character.Vital.Health); // Vida não muda
    }
}