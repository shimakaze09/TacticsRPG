using UnityEngine;

/// <summary>
/// What a fully decayed KO leaves on the board: a memory-core (restores half
/// of the collector's missing HP/MP) or salvage (grants scrip). Does not
/// occupy the tile, so any unit can end its move there to collect — handled
/// by MoveSequenceState. The fallen unit itself is removed from battle.
/// </summary>
public class RemainsPickup : MonoBehaviour
{
    public enum RemainsType
    {
        MemoryCore,
        Salvage
    }

    public RemainsType type;
    public Tile tile;

    [Tooltip("Scrip granted when this is salvage")]
    public int scripValue;

    /// <summary>
    /// Creates the pickup marker on the fallen unit's tile: a memory-core for
    /// cores (cyan), salvage crate otherwise (yellow); salvage value scales
    /// with the fallen unit's level.
    /// </summary>
    public static RemainsPickup Spawn(Unit fallen, bool asCore)
    {
        if (fallen == null || fallen.tile == null)
            return null;

        var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = asCore ? "Memory-core" : "Salvage";
        marker.transform.localScale = Vector3.one * 0.45f;
        marker.transform.position = fallen.tile.center + Vector3.up * 0.25f;

        var renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = asCore ? new Color(0.3f, 0.9f, 1f) : new Color(1f, 0.85f, 0.2f);

        var pickup = marker.AddComponent<RemainsPickup>();
        pickup.type = asCore ? RemainsType.MemoryCore : RemainsType.Salvage;
        pickup.tile = fallen.tile;

        var stats = fallen.GetComponent<Stats>();
        pickup.scripValue = 20 + (stats != null ? stats[StatTypes.LVL] * 5 : 0);

        return pickup;
    }

    /// <summary>The pickup sitting on a tile, if any.</summary>
    public static RemainsPickup FindAt(Tile tile)
    {
        if (tile == null)
            return null;

        foreach (var pickup in FindObjectsByType<RemainsPickup>())
        {
            if (pickup.tile == tile)
                return pickup;
        }

        return null;
    }

    /// <summary>
    /// Applies the payload to the collecting unit and removes the pickup:
    /// memory-cores restore half of missing HP/MP, salvage pays scrip.
    /// </summary>
    public void Collect(Unit collector)
    {
        if (collector == null)
            return;

        if (type == RemainsType.MemoryCore)
        {
            var stats = collector.GetComponent<Stats>();
            if (stats != null)
            {
                var hp = stats[StatTypes.HP] + (stats[StatTypes.MHP] - stats[StatTypes.HP]) / 2;
                var mp = stats[StatTypes.MP] + (stats[StatTypes.MMP] - stats[StatTypes.MP]) / 2;
                stats.SetValue(StatTypes.HP, hp, false);
                stats.SetValue(StatTypes.MP, mp, false);
            }

            Debug.Log($"[Remains] {collector.name} absorbed a memory-core");
        }
        else
        {
            Bank.Instance.gold += scripValue;
            Debug.Log($"[Remains] {collector.name} collected salvage worth {scripValue} scrip");
        }

        Destroy(gameObject);
    }
}
