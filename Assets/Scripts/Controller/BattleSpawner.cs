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
        var tile = FindPlacementTile(bc.board, entry.position, PlacementMask(instance));
        if (tile == null)
        {
            Debug.LogError($"[BattleSpawner] No free tile near {entry.position.x},{entry.position.y} for '{entry.recipe}'");
            Object.Destroy(instance);
            return null;
        }

        ApplyOverrides(instance, entry);

        unit.Place(tile);
        unit.dir = entry.facing;
        unit.Match();
        bc.units.Add(unit);
        return unit;
    }

    // Boss-tuning overrides (issue #52): swap the job, raise the grade, or
    // replace the gear loadout, then recalculate so the spawn's combat stats
    // reflect the authored intent — full HP/MP for the fresh unit
    private static void ApplyOverrides(GameObject instance, SpawnEntry entry)
    {
        var hasJob = !string.IsNullOrEmpty(entry.jobOverride);
        var hasGrade = entry.gradeOverride > 0;
        var hasGear = entry.gearOverride != null && entry.gearOverride.Count > 0;
        if (!hasJob && !hasGrade && !hasGear)
            return;

        var jm = instance.GetComponent<JobManager>();
        if (jm == null)
            return;

        if (hasJob)
        {
            JobDefinition job = null;
            foreach (var candidate in jm.allJobs)
            {
                if (candidate != null && candidate.id == entry.jobOverride)
                {
                    job = candidate;
                    break;
                }
            }

            if (job == null)
            {
                Debug.LogError($"[BattleSpawner] jobOverride '{entry.jobOverride}' not found for '{entry.recipe}'");
            }
            else
            {
                jm.ProgressData.UnlockJob(job);
                jm.ProgressData.SwitchJob(job);
            }
        }

        if (hasGrade && jm.CurrentJob != null)
            jm.ProgressData.SetJobLevel(jm.CurrentJob, Mathf.Clamp(entry.gradeOverride, 1, JobDefinition.MaxJobLevel));

        if (hasGear)
        {
            var equipment = instance.GetComponent<Equipment>();
            if (equipment != null)
            {
                for (var i = equipment.items.Count - 1; i >= 0; i--)
                {
                    var item = equipment.items[i];
                    equipment.UnEquip(item);
                    Object.Destroy(item.gameObject);
                }

                foreach (var gearId in entry.gearOverride)
                {
                    var item = ItemFactory.Create(gearId);
                    if (item == null)
                    {
                        Debug.LogError($"[BattleSpawner] gearOverride '{gearId}' not found for '{entry.recipe}'");
                        continue;
                    }

                    item.transform.SetParent(instance.transform);
                    var equippable = item.GetComponent<Equippable>();
                    equipment.Equip(equippable, equippable.defaultSlots);
                }
            }
        }

        jm.RecalculateStats(true);
    }

    /// <summary>Terrain the unit's locomotion may stand on, for placement.</summary>
    public static TileTraversalFlags PlacementMask(GameObject instance)
    {
        var movement = instance.GetComponent<Movement>();
        return movement != null ? movement.TraversalCapability : TileTraversalFlags.Ground;
    }

    // The authored tile if the unit can stand there, else the closest tile
    // it can
    private static Tile FindPlacementTile(Board board, Point position, TileTraversalFlags mask)
    {
        var tile = board.GetTile(position);
        if (tile != null && tile.content == null && tile.CanStop(mask))
            return tile;

        Tile best = null;
        var bestDistance = int.MaxValue;
        foreach (var candidate in board.tiles.Values)
        {
            if (candidate.content != null || !candidate.CanStop(mask))
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
