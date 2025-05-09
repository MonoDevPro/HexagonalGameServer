using Server.Domain.Events.Account;
using Server.Domain.Events.Player;

namespace Server.Application.Ports.Outbound;

/// <summary>
/// Interface for publishing player-related events to external systems
/// </summary>
public interface IPlayerEventPublisher
{
    // Connection events
    Task PublishAsync(PlayerConnectedEvent eventData);
    Task PublishAsync(PlayerDisconnectedEvent eventData);
    
    // Account events
    Task PublishAsync(PlayerAccountCreationSuccessEvent eventData);
    Task PublishAsync(PlayerAccountCreationFailedEvent eventData);
    Task PublishAsync(PlayerAccountLoginSuccessEvent eventData);
    Task PublishAsync(PlayerAccountLoginFailedEvent eventData);
    
    // Character events
    Task PublishAsync(PlayerCharacterCreationSuccessEvent eventData);
    Task PublishAsync(PlayerCharacterCreationFailedEvent eventData);
    Task PublishAsync(PlayerCharacterSelectSuccessEvent eventData);
    Task PublishAsync(PlayerCharacterSelectFailedEvent eventData);
    Task PublishAsync(PlayerCharacterEnteredWorldEvent eventData);
    Task PublishAsync(PlayerCharacterLeftWorldEvent eventData);
    
    // Gameplay events
    Task PublishAsync(PlayerCharacterMovedEvent eventData);
    Task PublishAsync(PlayerCharacterAttackedEvent eventData);
    Task PublishAsync(PlayerChatEvent eventData);
    Task PublishAsync(PlayerItemUsedEvent eventData);
    
    // Generic publish method for any player event
    Task PublishAsync(PlayerEvent eventData);
}
