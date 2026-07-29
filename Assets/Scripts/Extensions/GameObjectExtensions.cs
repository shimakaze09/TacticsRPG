using UnityEngine;

/// <summary>
/// GameObject helpers, e.g. AddChildComponent: create a parented child object
/// and attach a component in one call.
/// </summary>
public static class GameObjectExtensions
{
    public static T AddChildComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        var child = new GameObject(typeof(T).Name);
        child.transform.SetParent(obj.transform);
        return child.AddComponent<T>();
    }
}