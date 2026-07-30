using UnityEngine;

/// <summary>
/// Spawns units into a running battle from SpawnEntry data — used by
/// authored battle setup and reinforcement waves. Falls back to the nearest
/// free tile when the authored position is occupied or missing.
/// </summary>
public static class BattleSpawner
{
    /// <summary>
    /// Creates the unit, places it on (or near) its authored tile, and
    /// registers it with the battle. Returns null when the recipe fails.
    /// </summary>
    public static Unit Spawn(BattleController bc, SpawnEntry entry, Transform container)
    {
        var instance = UnitFactory.Create(entry.recipe, entry.level);
        if (instance == null)
        {
            Debug.LogError($"[BattleSpawner] Failed to spawn '{entry.recipe}'");
            return null;
        }

        if (container != null)
            instance.transform.SetParent(container);

        var unit = instance.GetComponent<Unit>();
        var tile = FindPlacementTile(bc.board, entry.position);
        if (tile == null)
        {
            Debug.LogError($"[BattleSpawner] No free tile near {entry.position.x},{entry.position.y} for '{entry.recipe}'");
            Object.Destroy(instance);
            return null;
        }

        unit.Place(tile);
        unit.dir = entry.facing;
        unit.Match();
        bc.units.Add(unit);
        return unit;
    }

    // The authored tile if free, else the closest free tile to it
    private static Tile FindPlacementTile(Board board, Point position)
    {
        var tile = board.GetTile(position);
        if (tile != null && tile.content == null)
            return tile;

        Tile best = null;
        var bestDistance = int.MaxValue;
        foreach (var candidate in board.tiles.Values)
        {
            if (candidate.content != null)
                continue;

            var distance = Mathf.Abs(candidate.pos.x - position.x) +
                           Mathf.Abs(candidate.pos.y - position.y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }
}
