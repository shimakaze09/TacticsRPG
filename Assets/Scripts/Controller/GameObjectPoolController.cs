using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolController : MonoBehaviour
{
    #region MonoBehaviour

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
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

    public static void SetMaxCount(string key, int maxCount)
    {
        if (!pools.ContainsKey(key))
            return;
        var data = pools[key];
        data.maxCount = maxCount;
    }

    public static bool AddEntry(string key, GameObject prefab, int prepopulate, int maxCount)
    {
        if (pools.ContainsKey(key))
            return false;

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

    public static void ClearEntry(string key)
    {
        if (!pools.ContainsKey(key))
            return;

        var data = pools[key];
        while (data.pool.Count > 0)
        {
            var obj = data.pool.Dequeue();
            Destroy(obj.gameObject);
        }

        pools.Remove(key);
    }

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

    public static Poolable Dequeue(string key)
    {
        if (!pools.ContainsKey(key))
            return null;

        var data = pools[key];
        if (data.pool.Count == 0)
            return CreateInstance(key, data.prefab);

        var obj = data.pool.Dequeue();
        obj.isPooled = false;
        return obj;
    }

    #endregion

    #region Private

    private static void CreateSharedInstance()
    {
        var obj = new GameObject("GameObject Pool Controller");
        DontDestroyOnLoad(obj);
        instance = obj.AddComponent<GameObjectPoolController>();
    }

    private static Poolable CreateInstance(string key, GameObject prefab)
    {
        var instance = Instantiate(prefab);
        var p = instance.AddComponent<Poolable>();
        p.key = key;
        return p;
    }

    #endregion
}