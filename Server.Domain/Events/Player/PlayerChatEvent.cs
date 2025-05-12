
namespace Server.Domain.Events.Player;

/// <summary>
/// Evento emitido quando um jogador envia uma mensagem de chat
/// </summary>
public class PlayerChatEvent : PlayerEvent
{
    /// <summary>
    /// Username do jogador
    /// </summary>
    public string Username { get; }
    
    /// <summary>
    /// Nome do personagem, se estiver com um ativo
    /// </summary>
    public string CharacterName { get; }
    
    /// <summary>
    /// Mensagem de chat enviada
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Momento em que a mensagem foi enviada
    /// </summary>
    public DateTime SentAt { get; }

    public PlayerChatEvent(
        int connectionId,
        string username,
        string characterName,
        string message)
        : base(connectionId)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        CharacterName = characterName ?? throw new ArgumentNullException(nameof(characterName));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        SentAt = DateTime.UtcNow;
    }
}