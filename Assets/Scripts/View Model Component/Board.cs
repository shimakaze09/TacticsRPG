using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    public Dictionary<Point, Tile> tiles = new();

    public void Load(LevelData data)
    {
        foreach (var tile in data.tiles)
        {
            var instance = Instantiate(tilePrefab);
            var t = instance.GetComponent<Tile>();
            t.Load(tile);
            tiles.Add(t.pos, t);
        }
    }
}