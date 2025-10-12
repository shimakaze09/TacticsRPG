using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public List<Vector3> tiles;
    public Utils.SerializableDictionary<Vector3, string> tileSkins;
}