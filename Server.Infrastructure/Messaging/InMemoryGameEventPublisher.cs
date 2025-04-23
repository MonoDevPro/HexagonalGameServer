using Server.Domain.Events;
using Server.Domain.Services;
using System.Collections.Concurrent;

namespace Server.Infrastructure.Messaging;

public class InMemoryGameEventPublisher : IGameEventPublisher
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    
    public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
    {
        var eventType = domainEvent.GetType();
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (handler is Func<TEvent, Task> asyncHandler)
                {
                    await asyncHandler(domainEvent);
                }
                else if (handler is Action<TEvent> syncHandler)
                {
                    syncHandler(domainEvent);
                }
            }
        }
    }
    
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            _ => new List<Delegate> { handler },
            (_, existingHandlers) =>
            {
                existingHandlers.Add(handler);
                return existingHandlers;
            });
    }
    
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            _ => new List<Delegate> { handler },
            (_, existingHandlers) =>
            {
                existingHandlers.Add(handler);
                return existingHandlers;
            });
    }
    
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
}