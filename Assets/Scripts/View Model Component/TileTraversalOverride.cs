using UnityEngine;

public class TileTraversalOverride : MonoBehaviour
{
    [SerializeField] private TileTraversalFlags overrideFlags = TileTraversalFlags.All;
    [SerializeField] private bool applyOnEnable;

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
