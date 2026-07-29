using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset describing a board: tile positions/heights and per-tile skin prefab
/// names.
/// </summary>
public class LevelData : ScriptableObject
{
    public List<Vector3> tiles;
    public Utils.SerializableDictionary<Vector3, string> tileSkins;
}