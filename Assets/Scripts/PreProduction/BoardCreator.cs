using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class BoardCreator : MonoBehaviour
{
    #region Fields / Properties

    [SerializeField] private GameObject tileViewPrefab;
    [SerializeField] private GameObject tileSelectionIndicatorPrefab;
    [SerializeField] private readonly int width = 10;
    [SerializeField] private readonly int depth = 10;
    [SerializeField] private readonly int height = 8;
    [SerializeField] private Point _pos;
    [SerializeField] private LevelData levelData;
    [SerializeField] private string levelName;
    private readonly Dictionary<Point, Tile> tiles = new();
    private const string _defaultSkin = "Tile";

    public Point pos
    {
        get => _pos;
        set => _pos = value;
    }

    private Transform marker
    {
        get
        {
            if (_marker == null)
            {
                var instance = Instantiate(tileSelectionIndicatorPrefab);
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
        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
        tiles.Clear();
        levelName = "";
    }

    public void Save()
    {
        var filePath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(filePath))
            CreateSaveDirectory();
        var board = ScriptableObject.CreateInstance<LevelData>();
        board.tiles = new List<Vector3>(tiles.Count);
        board.tileSkins = new Utils.SerializableDictionary<Vector3, string>();

        foreach (var t in tiles.Values)
        {
            var pos = new Vector3(t.pos.x, t.height, t.pos.y);
            board.tiles.Add(pos);

            var prefabName = t.name;
            prefabName = prefabName[..^7];
            board.tileSkins.Add(pos, prefabName);
        }

        var fileName = $"Assets/Resources/Levels/{levelName}.asset";
        AssetDatabase.CreateAsset(board, fileName);
    }

    public void Load()
    {
        Clear();
        if (levelData == null)
            return;

        levelName = levelData.name;

        foreach (var key in levelData.tiles)
        {
            levelData.tileSkins.TryGetValue(key, out var prefabName);
            prefabName ??= _defaultSkin;
            var variableForPrefab = (GameObject)Resources.Load("Prefabs/Blocks/" + prefabName, typeof(GameObject));
            var instance = Instantiate(variableForPrefab);
            instance.transform.SetParent(gameObject.transform);
            var t = instance.GetComponent<Tile>();
            t.Load(key);
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
        for (var y = (int)rect.yMin; y < (int)rect.yMax; y++)
        for (var x = (int)rect.xMin; x < (int)rect.xMax; x++)
        {
            var p = new Point(x, y);
            GrowSingle(p);
        }
    }

    private void ShrinkRect(Rect rect)
    {
        for (var y = (int)rect.yMin; y < (int)rect.yMax; y++)
        for (var x = (int)rect.xMin; x < (int)rect.xMax; x++)
        {
            var p = new Point(x, y);
            ShrinkSingle(p);
        }
    }

    private Tile Create()
    {
        var instance = Instantiate(tileViewPrefab);
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
#endif