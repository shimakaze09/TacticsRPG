using System;

/// <summary>
/// Extension methods to make the Event Bus more convenient to use.
/// Provides fluent API for publishing and subscribing to events.
/// </summary>
public static class EventBusExtensions
{
    private static GameEventBus Bus => GameEventBus.Instance;

    #region Publishing Extensions

    /// <summary>
    /// Publish an event from this object as the sender.
    /// </summary>
    public static void Publish<T>(this object sender, T eventData) where T : class
    {
        Bus.Publish(eventData, sender);
    }

    /// <summary>
    /// Publish an event with no specific sender.
    /// </summary>
    public static void PublishGlobal<T>(this object _, T eventData) where T : class
    {
        Bus.Publish(eventData, null);
    }

    #endregion

    #region Subscription Extensions

    /// <summary>
    /// Subscribe to events of type T from any sender.
    /// </summary>
    public static void Subscribe<T>(this object _, Action<T> handler) where T : class
    {
        Bus.Subscribe(handler);
    }

    /// <summary>
    /// Subscribe to events of type T from a specific sender.
    /// </summary>
    public static void SubscribeToSender<T>(this object _, Action<T> handler, object sender) where T : class
    {
        Bus.Subscribe(handler, sender);
    }

    /// <summary>
    /// Unsubscribe from events of type T from any sender.
    /// </summary>
    public static void Unsubscribe<T>(this object _, Action<T> handler) where T : class
    {
        Bus.Unsubscribe(handler);
    }

    /// <summary>
    /// Unsubscribe from events of type T from a specific sender.
    /// </summary>
    public static void UnsubscribeFromSender<T>(this object _, Action<T> handler, object sender) where T : class
    {
        Bus.Unsubscribe(handler, sender);
    }

    #endregion

    #region Service Locator Extensions

    /// <summary>
    /// Get a service from the Service Locator.
    /// </summary>
    public static T GetService<T>(this object _) where T : class
    {
        return ServiceLocator.Instance.Get<T>();
    }

    /// <summary>
    /// Try to get a service from the Service Locator.
    /// </summary>
    public static bool TryGetService<T>(this object _, out T service) where T : class
    {
        return ServiceLocator.Instance.TryGet(out service);
    }

    #endregion
}
