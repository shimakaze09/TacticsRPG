using UnityEngine;

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
    public Vector3 center => new(pos.x, height * stepHeight, pos.y);
    public GameObject content;
    [HideInInspector] public Tile prev;
    [HideInInspector] public int distance;
    [SerializeField] private TileTraversalFlags allowedTraversals = TileTraversalFlags.Ground | TileTraversalFlags.Fly | TileTraversalFlags.Teleport;
    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;
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

    public bool AllowsTraversal(TileTraversalFlags flags)
    {
        return (allowedTraversals & flags) != 0;
    }

    public void SetAllowedTraversals(TileTraversalFlags flags)
    {
        allowedTraversals = flags;
    }

    public void SetColor(Color color)
    {
        if (cachedRenderer == null)
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;
        }

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetColor(BaseColorId, color);
        cachedRenderer.SetPropertyBlock(propertyBlock);
    }

    #endregion
}