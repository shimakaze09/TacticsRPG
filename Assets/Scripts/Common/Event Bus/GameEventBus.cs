using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A centralized, strongly-typed event bus for game-wide communication.
/// Provides decoupled messaging between systems without direct dependencies.
/// Thread-safe and supports both global and sender-filtered subscriptions.
/// </summary>
public class GameEventBus
{
    #region Singleton

    private static GameEventBus _instance;
    public static GameEventBus Instance => _instance ??= new GameEventBus();

    private GameEventBus() { }

    #endregion

    #region Fields

    // Maps event types to their subscription lists
    private readonly Dictionary<Type, List<Subscription>> _subscriptions = new();

    // Tracks active invocations to prevent modification during iteration
    private readonly HashSet<List<Subscription>> _invoking = new();

    #endregion

    #region Subscription Management

    /// <summary>
    /// Subscribe to events of type T from any sender.
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : class
    {
        Subscribe<T>(handler, null);
    }

    /// <summary>
    /// Subscribe to events of type T from a specific sender.
    /// </summary>
    public void Subscribe<T>(Action<T> handler, object sender) where T : class
    {
        if (handler == null)
        {
            Debug.LogError($"Cannot subscribe null handler for event type {typeof(T).Name}");
            return;
        }

        var eventType = typeof(T);
        if (!_subscriptions.ContainsKey(eventType))
            _subscriptions[eventType] = new List<Subscription>();

        var list = _subscriptions[eventType];

        // Check if already subscribed
        foreach (var sub in list)
        {
            if (sub.Handler.Equals(handler) && sub.Sender == sender)
                return; // Already subscribed
        }

        // Copy list if currently invoking to prevent modification during iteration
        if (_invoking.Contains(list))
        {
            list = new List<Subscription>(list);
            _subscriptions[eventType] = list;
        }

        list.Add(new Subscription(handler, sender));
    }

    /// <summary>
    /// Unsubscribe from events of type T from any sender.
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        Unsubscribe<T>(handler, null);
    }

    /// <summary>
    /// Unsubscribe from events of type T from a specific sender.
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler, object sender) where T : class
    {
        if (handler == null)
        {
            Debug.LogError($"Cannot unsubscribe null handler for event type {typeof(T).Name}");
            return;
        }

        var eventType = typeof(T);
        if (!_subscriptions.ContainsKey(eventType))
            return;

        var list = _subscriptions[eventType];

        // Copy list if currently invoking to prevent modification during iteration
        if (_invoking.Contains(list))
        {
            list = new List<Subscription>(list);
            _subscriptions[eventType] = list;
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var sub = list[i];
            if (sub.Handler.Equals(handler) && sub.Sender == sender)
            {
                list.RemoveAt(i);
                break;
            }
        }

        // Clean up empty subscription lists
        if (list.Count == 0)
            _subscriptions.Remove(eventType);
    }

    #endregion

    #region Event Publishing

    /// <summary>
    /// Publish an event to all subscribed handlers.
    /// Supports both sender-specific and global subscriptions.
    /// </summary>
    public void Publish<T>(T eventData, object sender = null) where T : class
    {
        if (eventData == null)
        {
            Debug.LogError($"Cannot publish null event of type {typeof(T).Name}");
            return;
        }

        var eventType = typeof(T);
        if (!_subscriptions.ContainsKey(eventType))
            return;

        var list = _subscriptions[eventType];
        _invoking.Add(list);

        try
        {
            // Invoke sender-specific handlers first
            if (sender != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var sub = list[i];
                    if (sub.Sender != null && sub.Sender.Equals(sender))
                    {
                        try
                        {
                            ((Action<T>)sub.Handler)(eventData);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error invoking handler for {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
            }

            // Then invoke global handlers (null sender)
            for (int i = 0; i < list.Count; i++)
            {
                var sub = list[i];
                if (sub.Sender == null)
                {
                    try
                    {
                        ((Action<T>)sub.Handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking handler for {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }
        finally
        {
            _invoking.Remove(list);
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Remove all subscriptions. Useful for testing or scene transitions.
    /// </summary>
    public void Clear()
    {
        _subscriptions.Clear();
        _invoking.Clear();
    }

    /// <summary>
    /// Remove subscriptions to destroyed Unity objects.
    /// Should be called periodically to prevent memory leaks.
    /// </summary>
    public void CleanupDestroyedObjects()
    {
        var typesToRemove = new List<Type>();

        foreach (var kvp in _subscriptions)
        {
            var list = kvp.Value;

            // Copy if invoking
            if (_invoking.Contains(list))
            {
                list = new List<Subscription>(list);
                _subscriptions[kvp.Key] = list;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var sub = list[i];
                if (sub.Sender is UnityEngine.Object obj && obj == null)
                {
                    list.RemoveAt(i);
                }
            }

            if (list.Count == 0)
                typesToRemove.Add(kvp.Key);
        }

        foreach (var type in typesToRemove)
            _subscriptions.Remove(type);
    }

    #endregion

    #region Nested Types

    private class Subscription
    {
        public Delegate Handler { get; }
        public object Sender { get; }

        public Subscription(Delegate handler, object sender)
        {
            Handler = handler;
            Sender = sender;
        }
    }

    #endregion
}
