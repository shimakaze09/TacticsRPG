using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class LevelData : ScriptableObject
{
    public List<Vector3> tiles;
    public Utils.SerializableDictionary<Vector3, string> tileSkins;
}