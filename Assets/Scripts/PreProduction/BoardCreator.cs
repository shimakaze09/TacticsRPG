using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class BoardCreator : MonoBehaviour
{
    #region Fields / Properties

    [SerializeField] private GameObject tileViewPrefab;
    [SerializeField] private GameObject tileSelectionIndicatorPrefab;
    [SerializeField] private int width = 10;
    [SerializeField] private int depth = 10;
    [SerializeField] private int height = 8;
    [SerializeField] private LevelData levelData;
    private Dictionary<Point, Tile> tiles = new();

    public Point pos
    {
        get => pos;
        set => pos = value;
    }

    private Transform marker
    {
        get
        {
            if (_marker == null)
            {
                var instance = Instantiate(tileSelectionIndicatorPrefab) as GameObject;
                _marker = instance.transform;
            }

            return _marker;
        }
    }

    private Transform _marker;

    #endregion

    #region Public

    public void Grow()
    {
        GrowSingle(pos);
    }

    public void Shrink()
    {
        ShrinkSingle(pos);
    }

    public void GrowArea()
    {
        var r = RandomRect();
        GrowRect(r);
    }

    public void ShrinkArea()
    {
        var r = RandomRect();
        ShrinkRect(r);
    }

    public void UpdateMarker()
    {
        var t = tiles.ContainsKey(pos) ? tiles[pos] : null;
        marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0, pos.y);
    }

    public void Clear()
    {
        for (var i = transform.childCount - 1; i >= 0; --i)
            DestroyImmediate(transform.GetChild(i).gameObject);
        tiles.Clear();
    }

    public void Save()
    {
        var filePath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(filePath))
            CreateSaveDirectory();

        var board = ScriptableObject.CreateInstance<LevelData>();
        board.tiles = new List<Vector3>(tiles.Count);
        foreach (var t in tiles.Values)
            board.tiles.Add(new Vector3(t.pos.x, t.height, t.pos.y));

        var fileName = string.Format("Assets/Resources/Levels/{1}.asset", filePath, name);
        AssetDatabase.CreateAsset(board, fileName);
    }

    public void Load()
    {
        Clear();
        if (levelData == null)
            return;

        foreach (var v in levelData.tiles)
        {
            var t = Create();
            t.Load(v);
            tiles.Add(t.pos, t);
        }
    }

    public void CreateBase()
    {
        Clear();

        for (var i = 0; i < width; i++)
        for (var j = 0; j < depth; j++)
            GrowSingle(new Point(i, j));
    }

    #endregion

    #region Private

    private Rect RandomRect()
    {
        var x = Random.Range(0, width);
        var y = Random.Range(0, depth);
        var w = Random.Range(1, width - x + 1);
        var h = Random.Range(1, depth - y + 1);
        return new Rect(x, y, w, h);
    }

    private void GrowRect(Rect rect)
    {
        for (var y = (int)rect.yMin; y < (int)rect.yMax; ++y)
        for (var x = (int)rect.xMin; x < (int)rect.xMax; ++x)
        {
            var p = new Point(x, y);
            GrowSingle(p);
        }
    }

    private void ShrinkRect(Rect rect)
    {
        for (var y = (int)rect.yMin; y < (int)rect.yMax; ++y)
        for (var x = (int)rect.xMin; x < (int)rect.xMax; ++x)
        {
            var p = new Point(x, y);
            ShrinkSingle(p);
        }
    }

    private Tile Create()
    {
        var instance = Instantiate(tileViewPrefab) as GameObject;
        instance.transform.parent = transform;
        return instance.GetComponent<Tile>();
    }

    private Tile GetOrCreate(Point p)
    {
        if (tiles.ContainsKey(p))
            return tiles[p];

        var t = Create();
        t.Load(p, 0);
        tiles.Add(p, t);

        return t;
    }

    private void GrowSingle(Point p)
    {
        var t = GetOrCreate(p);
        if (t.height < height)
            t.Grow();
    }

    private void ShrinkSingle(Point p)
    {
        if (!tiles.ContainsKey(p))
            return;

        var t = tiles[p];
        t.Shrink();

        if (t.height <= 0)
        {
            tiles.Remove(p);
            DestroyImmediate(t.gameObject);
        }
    }

    private void CreateSaveDirectory()
    {
        var filePath = Application.dataPath + "/Resources";
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets", "Resources");
        filePath += "/Levels";
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets/Resources", "Levels");
        AssetDatabase.Refresh();
    }

    #endregion
}