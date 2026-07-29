using UnityEngine;

/// <summary>
/// Tag component for pooled objects: remembers its pool key and in-use state
/// for GameObjectPoolController.
/// </summary>
public class Poolable : MonoBehaviour
{
    public bool isPooled;
    public string key;
}