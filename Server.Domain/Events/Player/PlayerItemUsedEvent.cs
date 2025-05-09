using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento emitido quando um jogador usa um item
/// </summary>
public class PlayerItemUsedEvent : PlayerSuccessEvent
{
    /// <summary>
    /// ID do personagem que usou o item
    /// </summary>
    public long CharacterId { get; }
    
    /// <summary>
    /// Nome do personagem
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// ID do item usado
    /// </summary>
    public int ItemId { get; }
    
    /// <summary>
    /// Nome do item usado
    /// </summary>
    public string ItemName { get; }
    
    /// <summary>
    /// Se o item foi consumido (removido do inventário após uso)
    /// </summary>
    public bool WasConsumed { get; }
    
    /// <summary>
    /// Efeito resultante do uso do item (cura, buff, etc.)
    /// </summary>
    public string Effect { get; }
    
    /// <summary>
    /// Momento em que o item foi usado
    /// </summary>
    public DateTime UsedAt { get; }

    public PlayerItemUsedEvent(
        int connectionId, 
        long characterId, 
        string characterName, 
        int itemId, 
        string itemName,
        bool wasConsumed = true,
        string effect = null)
        : base(connectionId, $"Character {characterName} used item {itemName}")
    {
        CharacterId = characterId;
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        ItemId = itemId;
        ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
        WasConsumed = wasConsumed;
        Effect = effect;
        UsedAt = DateTime.UtcNow;
    }
}