using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento emitido quando um personagem de jogador ataca outro personagem
/// </summary>
public class PlayerCharacterAttackedEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID do personagem atacante
    /// </summary>
    public long AttackerId { get; }
    
    /// <summary>
    /// Nome do personagem atacante
    /// </summary>
    public string AttackerName { get; }
    
    /// <summary>
    /// ID do personagem alvo
    /// </summary>
    public long TargetId { get; }
    
    /// <summary>
    /// Nome do personagem alvo (se disponível)
    /// </summary>
    public string TargetName { get; }
    
    /// <summary>
    /// Dano causado pelo ataque
    /// </summary>
    public double Damage { get; }
    
    /// <summary>
    /// Se o ataque foi crítico
    /// </summary>
    public bool IsCritical { get; }
    
    /// <summary>
    /// Se o ataque resultou na morte do alvo
    /// </summary>
    public bool IsKillingBlow { get; }
    
    /// <summary>
    /// Momento do ataque
    /// </summary>
    public DateTime AttackTimestamp { get; }

    public PlayerCharacterAttackedEvent(
        int connectionId, 
        long attackerId, 
        string attackerName, 
        long targetId, 
        string targetName, 
        double damage, 
        bool isCritical = false, 
        bool isKillingBlow = false)
        : base(connectionId, $"Character {attackerName} attacked {targetName}")
    {
        AttackerId = attackerId;
        AttackerName = attackerName ?? throw new ArgumentNullException(nameof(attackerName));
        TargetId = targetId;
        TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        Damage = damage;
        IsCritical = isCritical;
        IsKillingBlow = isKillingBlow;
        AttackTimestamp = DateTime.UtcNow;
    }
}