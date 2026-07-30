using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset describing a board: tile positions/heights, per-tile skin prefab
/// names, and per-tile terrain types (index-aligned with tiles; levels saved
/// before terrain existed infer their types from skin names on load).
/// </summary>
public class LevelData : ScriptableObject
{
    public List<Vector3> tiles;
    public Utils.SerializableDictionary<Vector3, string> tileSkins;
    public List<int> tileTerrains;
}