using UnityEngine;

public class TileTraversalOverride : MonoBehaviour
{
    [SerializeField] private bool applyOnEnable;
    [SerializeField] private readonly TileTraversalFlags overrideFlags = TileTraversalFlags.Ground | TileTraversalFlags.Fly | TileTraversalFlags.Teleport;

    private void Awake()
    {
        Apply();
    }

    private void OnEnable()
    {
        if (applyOnEnable)
            Apply();
    }

    private void Apply()
    {
        var tile = GetComponentInParent<Tile>();
        if (tile == null)
            return;

        tile.SetAllowedTraversals(overrideFlags);
    }
}