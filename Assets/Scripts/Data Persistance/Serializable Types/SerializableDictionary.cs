using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
            Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys (" + keys.Count +
                           ") does not match the amount of values (" + values.Count +
                           ") which indicates that something went wrong.");

        for (var i = 0; i < keys.Count; i++) Add(keys[i], values[i]);
    }
}