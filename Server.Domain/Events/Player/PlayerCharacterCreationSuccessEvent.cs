using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento disparado quando a criação de personagem é bem-sucedida
/// </summary>
public class PlayerCharacterCreationSuccessEvent : PlayerSuccessEvent
{
    /// <summary>
    /// Nome de usuário da conta
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Nome do personagem criado
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Data/hora da criação
    /// </summary>
    public DateTime CreatedAt { get; }

    public PlayerCharacterCreationSuccessEvent(int connectionId, string username, string characterName)
        : base(connectionId, $"Character '{characterName}' created successfully for account '{username}'")
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        CreatedAt = DateTime.UtcNow;
    }
}
