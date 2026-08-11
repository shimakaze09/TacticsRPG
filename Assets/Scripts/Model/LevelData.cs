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

    // Initialized so pre-skin legacy assets (Level_2 has no serialized
    // tileSkins block) deserialize to an empty map instead of null —
    // Board.Load and BoardCreator.Load index it without a null check
    public SerializableDictionary<Vector3, string> tileSkins = new SerializableDictionary<Vector3, string>();

    public List<int> tileTerrains;
}