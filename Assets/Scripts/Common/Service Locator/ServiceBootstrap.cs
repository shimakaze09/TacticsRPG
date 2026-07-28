using UnityEngine;

/// <summary>
/// Registers core services in the ServiceLocator before any scene loads.
/// Without this, ServiceLocator.Get&lt;GameEventBus&gt;() returns null and callers
/// that resolve the bus through the locator throw on first use.
/// </summary>
public static class ServiceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterCoreServices()
    {
        if (!ServiceLocator.Instance.IsRegistered<GameEventBus>())
            ServiceLocator.Instance.Register(GameEventBus.Instance);
    }
}
