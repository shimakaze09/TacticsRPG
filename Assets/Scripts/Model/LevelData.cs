using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileSkins : SerializableDictionary<Vector3, string>
{
}

public class LevelData : ScriptableObject
{
    public List<Vector3> tiles;
    public TileSkins tileSkins;
}