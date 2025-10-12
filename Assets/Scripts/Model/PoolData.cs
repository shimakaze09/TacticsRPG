using System.Collections.Generic;
using UnityEngine;

public class PoolData
{
    public int maxCount;
    public Queue<Poolable> pool;
    public GameObject prefab;
}