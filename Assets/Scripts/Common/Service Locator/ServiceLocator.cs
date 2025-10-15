using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service Locator pattern for dependency injection.
/// Provides centralized access to services without tight coupling.
/// Services can be registered, retrieved, and managed through this class.
/// </summary>
public class ServiceLocator
{
    #region Singleton

    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();

    private ServiceLocator() { }

    #endregion

    #region Fields

    private readonly Dictionary<Type, object> _services = new();

    #endregion

    #region Service Registration

    /// <summary>
    /// Register a service instance.
    /// </summary>
    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);

        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
        }

        _services[type] = service;
    }

    /// <summary>
    /// Register a service with a specific interface type.
    /// Useful when registering concrete types as interfaces.
    /// </summary>
    public void RegisterAs<TInterface, TImplementation>(TImplementation service)
        where TImplementation : class, TInterface
    {
        var type = typeof(TInterface);

        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
        }

        _services[type] = service;
    }

    /// <summary>
    /// Unregister a service.
    /// </summary>
    public void Unregister<T>() where T : class
    {
        var type = typeof(T);
        _services.Remove(type);
    }

    #endregion

    #region Service Retrieval

    /// <summary>
    /// Get a registered service. Returns null if not found.
    /// </summary>
    public T Get<T>() where T : class
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var service))
        {
            return service as T;
        }

        Debug.LogWarning($"Service of type {type.Name} not found.");
        return null;
    }

    /// <summary>
    /// Try to get a registered service. Returns true if found.
    /// </summary>
    public bool TryGet<T>(out T service) where T : class
    {
        var type = typeof(T);

        if (_services.TryGetValue(type, out var obj))
        {
            service = obj as T;
            return service != null;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Get a service or throw an exception if not found.
    /// Use when the service is required.
    /// </summary>
    public T GetRequired<T>() where T : class
    {
        var service = Get<T>();

        if (service == null)
        {
            throw new InvalidOperationException($"Required service of type {typeof(T).Name} not found.");
        }

        return service;
    }

    /// <summary>
    /// Check if a service is registered.
    /// </summary>
    public bool IsRegistered<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Clear all registered services.
    /// </summary>
    public void Clear()
    {
        _services.Clear();
    }

    /// <summary>
    /// Remove services that reference destroyed Unity objects.
    /// Should be called periodically to prevent memory leaks.
    /// </summary>
    public void CleanupDestroyedObjects()
    {
        var keysToRemove = new List<Type>();

        foreach (var kvp in _services)
        {
            if (kvp.Value is UnityEngine.Object obj && obj == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _services.Remove(key);
            Debug.LogWarning($"Removed destroyed service of type {key.Name}");
        }
    }

    #endregion
}
