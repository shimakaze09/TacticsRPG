using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    #region Fields / Properties

    [SerializeField] private GameObject tilePrefab;
    public Dictionary<Point, Tile> tiles = new();

    private Point[] dirs = new Point[4]
    {
        new(0, 1),
        new(0, -1),
        new(1, 0),
        new(-1, 0)
    };

    private Color selectedTileColor = new(0, 1, 1, 1);
    private Color defaultTileColor = new(1, 1, 1, 1);

    #endregion

    #region Public

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

    public Tile GetTile(Point p)
    {
        return tiles.ContainsKey(p) ? tiles[p] : null;
    }

    public List<Tile> Search(Tile start, Func<Tile, Tile, bool> addTile)
    {
        var retValue = new List<Tile> { start };

        ClearSearch();
        var checkNext = new Queue<Tile>();
        var checkNow = new Queue<Tile>();

        start.distance = 0;
        checkNow.Enqueue(start);

        while (checkNow.Count > 0)
        {
            var t = checkNow.Dequeue();
            for (var i = 0; i < 4; ++i)
            {
                var next = GetTile(t.pos + dirs[i]);
                if (next == null || next.distance <= t.distance + 1)
                    continue;

                if (addTile(t, next))
                {
                    next.distance = t.distance + 1;
                    next.prev = t;
                    checkNext.Enqueue(next);
                    retValue.Add(next);
                }
            }

            if (checkNow.Count == 0)
                SwapReference(ref checkNow, ref checkNext);
        }

        return retValue;
    }

    public void SelectTiles(List<Tile> tiles)
    {
        for (var i = tiles.Count - 1; i >= 0; --i)
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", selectedTileColor);
    }

    public void DeSelectTiles(List<Tile> tiles)
    {
        for (var i = tiles.Count - 1; i >= 0; --i)
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", defaultTileColor);
    }

    #endregion

    #region Private

    private void ClearSearch()
    {
        foreach (var t in tiles.Values)
        {
            t.prev = null;
            t.distance = int.MaxValue;
        }
    }

    private void SwapReference(ref Queue<Tile> a, ref Queue<Tile> b)
    {
        (a, b) = (b, a);
    }

    #endregion
}