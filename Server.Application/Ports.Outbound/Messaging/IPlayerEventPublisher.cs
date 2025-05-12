using Server.Domain.Events.Player;
using Server.Domain.Events.Player.Account;
using Server.Domain.Events.Player.Character;
using Server.Domain.Events.Player.Connection;

namespace Server.Application.Ports.Outbound.Messaging;

/// <summary>
/// Interface for publishing player-related events to external systems
/// </summary>
public interface IPlayerEventPublisher<TEvent>
    where TEvent : PlayerEvent
{
    // ConnectionEvents
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
    Task PublishAsync(PlayerChatEvent eventData);
}
