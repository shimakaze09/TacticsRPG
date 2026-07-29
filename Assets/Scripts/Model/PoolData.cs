using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bookkeeping for one GameObjectPoolController entry: prefab, size limit, and
/// the queue of inactive instances.
/// </summary>
public class PoolData
{
    public int maxCount;
    public Queue<Poolable> pool;
    public GameObject prefab;
}