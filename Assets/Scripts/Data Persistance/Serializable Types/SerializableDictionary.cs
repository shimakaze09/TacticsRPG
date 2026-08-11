using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The project's one dictionary that survives Unity/JsonUtility serialization,
/// flattening to parallel lists serialized as "keys"/"values" — the field
/// names shipped save files and level assets contain, so they must never be
/// renamed. Used by GameData save payloads and LevelData tile skins.
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    /// <summary>Flattens the live dictionary into the parallel lists Unity serializes.</summary>
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

    /// <summary>
    /// Rebuilds the dictionary from the serialized lists. Corrupt data must
    /// not abort a load: a count mismatch logs and keeps the pairs that
    /// exist, and duplicate keys resolve last-wins instead of throwing.
    /// </summary>
    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
            Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys (" + keys.Count +
                           ") does not match the amount of values (" + values.Count +
                           ") which indicates that something went wrong.");

        var count = Math.Min(keys.Count, values.Count);
        for (var i = 0; i < count; i++) this[keys[i]] = values[i];
    }
}
