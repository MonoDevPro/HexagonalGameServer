using System;

namespace Server.Domain.Events.Player;

/// <summary>
/// Evento emitido quando um jogador envia uma mensagem de chat
/// </summary>
public class PlayerChatEvent : PlayerSuccessEvent
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
    /// ID do personagem, se estiver com um ativo
    /// </summary>
    public long? CharacterId { get; }
    
    /// <summary>
    /// Mensagem de chat enviada
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Tipo do chat (global, local, grupo, sussurro)
    /// </summary>
    public ChatType Type { get; }
    
    /// <summary>
    /// Destinatário, caso seja um chat privado/sussurro
    /// </summary>
    public string RecipientName { get; }
    
    /// <summary>
    /// Momento em que a mensagem foi enviada
    /// </summary>
    public DateTime SentAt { get; }

    public PlayerChatEvent(
        int connectionId, 
        string username, 
        string message, 
        ChatType type = ChatType.Global, 
        string recipientName = null, 
        string characterName = null, 
        long? characterId = null)
        : base(connectionId, $"Player {username} sent a chat message")
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type;
        RecipientName = recipientName;
        CharacterName = characterName;
        CharacterId = characterId;
        SentAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Tipos de mensagens de chat disponíveis
/// </summary>
public enum ChatType
{
    /// <summary>
    /// Chat global, visível para todos os jogadores
    /// </summary>
    Global,
    
    /// <summary>
    /// Chat local, visível para jogadores próximos
    /// </summary>
    Local,
    
    /// <summary>
    /// Chat de grupo/equipe
    /// </summary>
    Party,
    
    /// <summary>
    /// Chat privado (sussurro)
    /// </summary>
    Whisper,
    
    /// <summary>
    /// Chat de sistema
    /// </summary>
    System,
    
    /// <summary>
    /// Chat de comércio
    /// </summary>
    Trade,
    
    /// <summary>
    /// Comando administrativo
    /// </summary>
    Admin
}