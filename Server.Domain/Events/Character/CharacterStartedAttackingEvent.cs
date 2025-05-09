using CharacterState = Server.Domain.Enum.CharacterState;

namespace Server.Domain.Events.Character;

/// <summary>
/// Evento disparado quando um personagem começa a atacar.
/// </summary>
public class CharacterStartedAttackingEvent : CharacterStateChangedEvent
{
    /// <summary>
    /// ID do alvo do ataque (se houver)
    /// </summary>
    public long? TargetId { get; }
    
    /// <summary>
    /// Tipo de ataque
    /// </summary>
    public string AttackType { get; }
    
    /// <summary>
    /// Dano potencial do ataque
    /// </summary>
    public int PotentialDamage { get; }
    
    /// <summary>
    /// Posição X de onde o ataque foi iniciado
    /// </summary>
    public float PositionX { get; }
    
    /// <summary>
    /// Posição Y de onde o ataque foi iniciado
    /// </summary>
    public float PositionY { get; }

    /// <summary>
    /// Cria uma nova instância do evento de início de ataque.
    /// </summary>
    /// <param name="previousState">Estado anterior do personagem</param>
    /// <param name="characterId">ID do personagem</param>
    /// <param name="targetId">ID do alvo (opcional)</param>
    /// <param name="attackType">Tipo de ataque</param>
    /// <param name="potentialDamage">Dano potencial</param>
    /// <param name="positionX">Posição X de onde o ataque foi iniciado</param>
    /// <param name="positionY">Posição Y de onde o ataque foi iniciado</param>
    public CharacterStartedAttackingEvent(
        CharacterState previousState,
        long characterId,
        string attackType,
        int potentialDamage,
        float positionX,
        float positionY,
        long? targetId = null) 
        : base(previousState, CharacterState.Attacking, characterId, 
              targetId.HasValue ? $"Começou a atacar o alvo {targetId} com {attackType}" : $"Começou a atacar com {attackType}")
    {
        TargetId = targetId;
        AttackType = attackType;
        PotentialDamage = potentialDamage;
        PositionX = positionX;
        PositionY = positionY;
    }
}