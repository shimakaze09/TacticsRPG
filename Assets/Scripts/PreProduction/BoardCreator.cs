using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// Editor-time board authoring tool: paint/raise/lower tiles, then save or load
/// LevelData assets.
/// </summary>
public class BoardCreator : MonoBehaviour
{
    #region Fields / Properties

    [SerializeField] private GameObject tileViewPrefab;
    [SerializeField] private GameObject tileSelectionIndicatorPrefab;
    [SerializeField] private readonly int width = 10;
    [SerializeField] private readonly int depth = 10;
    [SerializeField] private readonly int height = 8;
    [SerializeField] private Point _pos;
    [SerializeField] private TerrainType paintTerrain = TerrainType.Field;
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

    /// <summary>
    /// Repaints the tile under the marker as the selected terrain: swaps in
    /// the terrain's block prefab, keeping position and height.
    /// </summary>
    public void Paint()
    {
        if (!tiles.ContainsKey(pos))
            return;

        var old = tiles[pos];
        var replacement = SpawnTile(TerrainRules.Skin(paintTerrain));
        if (replacement == null)
            return;

        replacement.Load(pos, old.height);
        replacement.terrain = paintTerrain;

        tiles.Remove(pos);
        DestroyImmediate(old.gameObject);
        tiles.Add(pos, replacement);
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
        board.tileTerrains = new List<int>(tiles.Count);

        foreach (var t in tiles.Values)
        {
            var pos = new Vector3(t.pos.x, t.height, t.pos.y);
            board.tiles.Add(pos);

            var prefabName = t.name;
            prefabName = prefabName[..^7];
            board.tileSkins.Add(pos, prefabName);
            board.tileTerrains.Add((int)t.terrain);
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

        for (var i = 0; i < levelData.tiles.Count; i++)
        {
            var key = levelData.tiles[i];
            levelData.tileSkins.TryGetValue(key, out var prefabName);
            prefabName ??= _defaultSkin;

            var t = SpawnTile(prefabName);
            t.Load(key);
            // Same legacy fallback as Board.Load: infer terrain from the
            // skin when the asset predates terrain data
            t.terrain = levelData.tileTerrains != null && i < levelData.tileTerrains.Count
                ? (TerrainType)levelData.tileTerrains[i]
                : TerrainRules.FromSkin(prefabName);
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

    // Instantiates a block prefab by name, parented to the creator
    private Tile SpawnTile(string prefabName)
    {
        var prefab = (GameObject)Resources.Load("Prefabs/Blocks/" + prefabName, typeof(GameObject));
        if (prefab == null)
        {
            Debug.LogError($"No block prefab found for '{prefabName}'.");
            return null;
        }

        var instance = Instantiate(prefab);
        instance.transform.SetParent(transform);
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