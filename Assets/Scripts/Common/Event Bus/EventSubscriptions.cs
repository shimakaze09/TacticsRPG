using System;
using System.Collections.Generic;

/// <summary>
/// A tracked bag of event-bus subscriptions that guarantees symmetric
/// cleanup: every Subscribe made through the bag is undone by one Clear()
/// call. Components hold one, subscribe in OnEnable, and Clear in
/// OnDisable — no hand-maintained mirror lists, no leaked handlers
/// (ARCHITECTURE.md "Event bus rules").
/// </summary>
public class EventSubscriptions
{
    private readonly List<Action> unsubscribers = new List<Action>();

    /// <summary>Subscribe to T from any sender (remembered for Clear).</summary>
    public void Subscribe<T>(Action<T> handler) where T : class
    {
        GameEventBus.Instance.Subscribe(handler);
        unsubscribers.Add(() => GameEventBus.Instance.Unsubscribe(handler));
    }

    /// <summary>Subscribe to T from one sender (remembered for Clear).</summary>
    public void SubscribeToSender<T>(Action<T> handler, object sender) where T : class
    {
        GameEventBus.Instance.Subscribe(handler, sender);
        unsubscribers.Add(() => GameEventBus.Instance.Unsubscribe(handler, sender));
    }

    /// <summary>Undo every subscription made through this bag.</summary>
    public void Clear()
    {
        for (var i = unsubscribers.Count - 1; i >= 0; i--)
            unsubscribers[i]();
        unsubscribers.Clear();
    }
}
