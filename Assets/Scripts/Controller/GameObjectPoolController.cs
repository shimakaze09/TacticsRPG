using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static object pool: register a key+prefab, then dequeue/enqueue Poolable
/// instances instead of instantiating and destroying. The registry is static
/// while scene objects are not, so the controller makes itself persistent and
/// every access path tolerates entries whose objects died with an unloaded
/// scene (issue #23).
/// </summary>
public class GameObjectPoolController : MonoBehaviour
{
    #region MonoBehaviour

    // A scene-placed controller claims the shared instance and must survive
    // scene changes exactly like the lazily created one — otherwise the static
    // registry outlives its queued objects; duplicates destroy themselves
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Fields / Properties

    private static GameObjectPoolController Instance
    {
        get
        {
            if (instance == null)
                CreateSharedInstance();
            return instance;
        }
    }

    private static GameObjectPoolController instance;

    private static readonly Dictionary<string, PoolData> pools = new();

    #endregion

    #region Public

    /// <summary>Caps how many instances a pool retains; extras are destroyed on Enqueue.</summary>
    public static void SetMaxCount(string key, int maxCount)
    {
        if (!pools.ContainsKey(key))
            return;
        var data = pools[key];
        data.maxCount = maxCount;
    }

    /// <summary>
    /// Registers a pool. Re-registering the same key+prefab is a harmless
    /// no-op (returns false, e.g. a UI panel re-initializing after a scene
    /// load); the same key with a different prefab is a data error and is
    /// rejected loudly so two systems can never silently share a pool.
    /// </summary>
    public static bool AddEntry(string key, GameObject prefab, int prepopulate, int maxCount)
    {
        if (pools.TryGetValue(key, out var existing))
        {
            if (existing.prefab != prefab)
                Debug.LogError($"Pool key '{key}' already registered with prefab " +
                               $"'{(existing.prefab != null ? existing.prefab.name : "<destroyed>")}' — " +
                               $"refusing conflicting prefab '{(prefab != null ? prefab.name : "<null>")}'");
            return false;
        }

        var data = new PoolData
        {
            prefab = prefab,
            maxCount = maxCount,
            pool = new Queue<Poolable>(prepopulate)
        };
        pools.Add(key, data);

        for (var i = 0; i < prepopulate; i++)
            Enqueue(CreateInstance(key, prefab));

        return true;
    }

    /// <summary>Destroys a pool's queued instances and removes its registration.</summary>
    public static void ClearEntry(string key)
    {
        if (!pools.ContainsKey(key))
            return;

        var data = pools[key];
        while (data.pool.Count > 0)
        {
            var obj = data.pool.Dequeue();
            // Entries can die with an unloaded scene before the pool is cleared
            if (obj != null)
                Destroy(obj.gameObject);
        }

        pools.Remove(key);
    }

    /// <summary>
    /// Returns an instance to its pool: reparented under the persistent
    /// controller (so it survives scene unloads) and deactivated.
    /// </summary>
    public static void Enqueue(Poolable sender)
    {
        if (sender == null || sender.isPooled || !pools.ContainsKey(sender.key))
            return;

        var data = pools[sender.key];
        if (data.pool.Count >= data.maxCount)
        {
            Destroy(sender.gameObject);
            return;
        }

        data.pool.Enqueue(sender);
        sender.isPooled = true;
        sender.transform.SetParent(Instance.transform);
        sender.gameObject.SetActive(false);
    }

    /// <summary>
    /// Takes an instance from the pool, pruning any entry that was destroyed
    /// externally, and instantiates a fresh one when the pool runs dry.
    /// </summary>
    public static Poolable Dequeue(string key)
    {
        if (!pools.TryGetValue(key, out var data))
            return null;

        while (data.pool.Count > 0)
        {
            var obj = data.pool.Dequeue();
            if (obj == null)
                continue;
            obj.isPooled = false;
            return obj;
        }

        return CreateInstance(key, data.prefab);
    }

    #endregion

    #region Private

    // Lazily creates the persistent controller for code paths that never
    // placed one in a scene
    private static void CreateSharedInstance()
    {
        var obj = new GameObject("GameObject Pool Controller");
        DontDestroyOnLoad(obj);
        instance = obj.AddComponent<GameObjectPoolController>();
    }

    // Instantiates a pool member; a dead prefab reference is a registration
    // that outlived its asset and must fail loudly rather than throw
    private static Poolable CreateInstance(string key, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"Pool '{key}' has no live prefab — cannot create an instance");
            return null;
        }

        var instance = Instantiate(prefab);
        var p = instance.AddComponent<Poolable>();
        p.key = key;
        return p;
    }

    #endregion
}
