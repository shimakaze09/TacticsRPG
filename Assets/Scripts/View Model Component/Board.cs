using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    #region Fields / Properties

    public Dictionary<Point, Tile> tiles = new();
    public Point min => _min;
    public Point max => _max;
    private Point _min;
    private Point _max;

    private Point[] dirs = new Point[4]
    {
        new(0, 1),
        new(0, -1),
        new(1, 0),
        new(-1, 0)
    };

    private Color selectedTileColor = new(0, 1, 1, 1);
    private Color confirmedTileColor = new(1, 0.5f, 0.5f, 1);
    private Color defaultTileColor = new(1, 1, 1, 1);

    #endregion

    #region Public

    public void Load(LevelData data)
    {
        _min = new Point(int.MaxValue, int.MaxValue);
        _max = new Point(int.MinValue, int.MinValue);

        foreach (var key in data.tiles)
        {
            data.tileSkins.TryGetValue(key, out var prefabName);
            var variableForPrefab = (GameObject)Resources.Load("Prefabs/Blocks/" + prefabName, typeof(GameObject));
            var instance = Instantiate(variableForPrefab);
            var t = instance.GetComponent<Tile>();
            t.Load(key);

            tiles.Add(t.pos, t);

            _min.x = Mathf.Min(_min.x, t.pos.x);
            _min.y = Mathf.Min(_min.y, t.pos.y);
            _max.x = Mathf.Max(_max.x, t.pos.x);
            _max.y = Mathf.Max(_max.y, t.pos.y);
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
            for (var i = 0; i < 4; i++)
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
        foreach (var tile in tiles)
            tile.GetComponent<Renderer>().material.SetColor("_BaseColor", selectedTileColor);
    }

    public void ConfirmTiles(List<Tile> tiles)
    {
        foreach (var tile in tiles)
            tile.GetComponent<Renderer>().material.SetColor("_BaseColor", confirmedTileColor);
    }

    public void DeSelectTiles(List<Tile> tiles)
    {
        foreach (var tile in tiles)
            tile.GetComponent<Renderer>().material.SetColor("_BaseColor", defaultTileColor);
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