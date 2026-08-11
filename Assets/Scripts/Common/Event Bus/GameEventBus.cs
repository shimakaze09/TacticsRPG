using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A centralized, strongly-typed event bus for game-wide communication.
/// Contract (issue #24): **main-thread only** — enforced, not just assumed;
/// off-thread calls are rejected with an error. Subscriptions whose sender
/// or handler target is a destroyed Unity object are never invoked and are
/// pruned lazily on publish and swept on every scene unload (with a leak
/// warning, since a surviving subscription past unload means a component
/// skipped its EventSubscriptions.Clear()). Mutation-during-publish
/// semantics: a handler added during a publish is first invoked on the
/// next publish; a handler removed during a publish still receives the
/// in-flight event.
/// </summary>
public class GameEventBus
{
    #region Singleton

    private static GameEventBus _instance;
    public static GameEventBus Instance => _instance ??= new GameEventBus();

    private GameEventBus() { }

    // Captures the main thread and installs the scene-unload sweep before
    // any scene code runs
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Bootstrap()
    {
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // The runtime cleanup caller: every scene unload sweeps dead
    // subscriptions and flags them — they are leaks by definition
    private static void OnSceneUnloaded(Scene scene)
    {
        var removed = Instance.CleanupDestroyedObjects();
        if (removed > 0)
            Debug.LogWarning($"[EventBus] Pruned {removed} dead subscription(s) after unloading '{scene.name}' — a component skipped its EventSubscriptions.Clear() (ARCHITECTURE.md §2.5).");
    }

    #endregion

    #region Fields

    private static int _mainThreadId;

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
        if (!OnMainThread("Subscribe"))
            return;

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
        if (!OnMainThread("Unsubscribe"))
            return;

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
    /// Publish an event to all live subscribed handlers — sender-specific
    /// first, then global. Subscriptions whose sender or handler target has
    /// been destroyed are skipped and pruned after delivery.
    /// </summary>
    public void Publish<T>(T eventData, object sender = null) where T : class
    {
        if (!OnMainThread("Publish"))
            return;

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
        var deadSeen = false;

        try
        {
            // Invoke sender-specific handlers first
            if (sender != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var sub = list[i];
                    if (sub.IsDead)
                    {
                        deadSeen = true;
                        continue;
                    }

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
                if (sub.IsDead)
                {
                    deadSeen = true;
                    continue;
                }

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

        if (deadSeen)
            PruneDeadSubscriptions(eventType);
    }

    #endregion

    #region Cleanup / Diagnostics

    /// <summary>
    /// Remove all subscriptions. Useful for testing or scene transitions.
    /// </summary>
    public void Clear()
    {
        _subscriptions.Clear();
        _invoking.Clear();
    }

    /// <summary>
    /// Removes every subscription whose sender OR handler delegate target is
    /// a destroyed Unity object (global subscriptions have a null sender but
    /// still die with their component). Called automatically on scene
    /// unload; returns how many were pruned.
    /// </summary>
    public int CleanupDestroyedObjects()
    {
        var removed = 0;
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
                if (list[i].IsDead)
                {
                    list.RemoveAt(i);
                    removed++;
                }
            }

            if (list.Count == 0)
                typesToRemove.Add(kvp.Key);
        }

        foreach (var type in typesToRemove)
            _subscriptions.Remove(type);

        return removed;
    }

    /// <summary>
    /// Live subscription count for one event type (diagnostics/probes).
    /// </summary>
    public int CountSubscriptions(Type eventType)
    {
        if (eventType == null || !_subscriptions.TryGetValue(eventType, out var list))
            return 0;

        var count = 0;
        foreach (var sub in list)
            if (!sub.IsDead)
                count++;
        return count;
    }

    /// <summary>
    /// Total dead-but-resident subscriptions across all event types — each
    /// one is a component that skipped symmetric cleanup (diagnostics).
    /// </summary>
    public int CountDeadSubscriptions()
    {
        var count = 0;
        foreach (var kvp in _subscriptions)
            foreach (var sub in kvp.Value)
                if (sub.IsDead)
                    count++;
        return count;
    }

    // Prunes one event type's dead entries, respecting copy-on-write when a
    // (possibly nested) publish is still iterating the list
    private void PruneDeadSubscriptions(Type eventType)
    {
        if (!_subscriptions.TryGetValue(eventType, out var list))
            return;

        if (_invoking.Contains(list))
        {
            list = new List<Subscription>(list);
            _subscriptions[eventType] = list;
        }

        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i].IsDead)
                list.RemoveAt(i);

        if (list.Count == 0)
            _subscriptions.Remove(eventType);
    }

    // Enforces the documented main-thread-only contract; rejecting (rather
    // than racing) keeps the unsynchronized collections coherent
    private static bool OnMainThread(string operation)
    {
        if (_mainThreadId == 0 || Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            return true;

        Debug.LogError($"[EventBus] {operation} called off the main thread — the bus is main-thread only (issue #24); the call was ignored.");
        return false;
    }

    #endregion

    #region Nested Types

    // One handler registration; dead when its sender or delegate target is
    // a destroyed Unity object
    private class Subscription
    {
        public Delegate Handler { get; }
        public object Sender { get; }

        public bool IsDead =>
            (Sender is UnityEngine.Object senderObj && senderObj == null) ||
            (Handler.Target is UnityEngine.Object targetObj && targetObj == null);

        public Subscription(Delegate handler, object sender)
        {
            Handler = handler;
            Sender = sender;
        }
    }

    #endregion
}
