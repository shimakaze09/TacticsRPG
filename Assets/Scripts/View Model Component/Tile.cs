using UnityEngine;

/// <summary>
/// One board cell: grid position, height, terrain type, occupant, and the
/// search bookkeeping (prev/distance) used by pathfinding. Terrain gameplay
/// (pass/stop/sight) is answered here via TerrainRules.
/// </summary>
public class Tile : MonoBehaviour
{
    #region Const

    public const float stepHeight = 0.25f;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer == null)
            cachedRenderer = GetComponentInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        // Remember the skin's own color so selection tinting can blend with
        // it instead of erasing what terrain this is
        if (cachedRenderer != null && cachedRenderer.sharedMaterial != null &&
            cachedRenderer.sharedMaterial.HasProperty(BaseColorId))
            baseColor = cachedRenderer.sharedMaterial.GetColor(BaseColorId);
    }

    #endregion

    #region Private

    private void Match()
    {
        transform.localPosition = new Vector3(pos.x, height * stepHeight / 2f, pos.y);
        transform.localScale = new Vector3(1, height * stepHeight, 1);
    }

    #endregion

    #region Fields / Properties

    public Point pos;
    public int height;
    public TerrainType terrain = TerrainType.Field;
    public Vector3 center => new(pos.x, height * stepHeight, pos.y);
    public GameObject content;
    [HideInInspector] public Tile prev;
    [HideInInspector] public int distance;
    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color baseColor = Color.white;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    #endregion

    #region Public

    public void Grow()
    {
        height++;
        Match();
    }

    public void Shrink()
    {
        height--;
        Match();
    }

    public void Load(Point p, int h)
    {
        pos = p;
        height = h;
        Match();
    }

    public void Load(Vector3 v)
    {
        Load(new Point((int)v.x, (int)v.z), (int)v.y);
    }

    /// <summary>May this locomotion type path through the tile?</summary>
    public bool CanPass(TileTraversalFlags flags)
    {
        return (TerrainRules.Pass(terrain) & flags) != 0;
    }

    /// <summary>May this locomotion type end a move on the tile?</summary>
    public bool CanStop(TileTraversalFlags flags)
    {
        return (TerrainRules.Stop(terrain) & flags) != 0;
    }

    /// <summary>Does the terrain hide what's behind it from ranged attacks?</summary>
    public bool BlocksSight => TerrainRules.BlocksSight(terrain);

    // Selection tint: blends with the skin's own color, so white = normal
    public void SetColor(Color color)
    {
        if (cachedRenderer == null)
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;
        }

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetColor(BaseColorId, color * baseColor);
        cachedRenderer.SetPropertyBlock(propertyBlock);
    }

    #endregion
}