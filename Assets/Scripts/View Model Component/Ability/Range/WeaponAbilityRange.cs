using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Range for basic weapon attacks, read live from the equipped weapon's
/// GearCatalog entry (unarmed = melee 1). Reach, fire arc, and shape all
/// follow the weapon: direct fire is blocked by standing units and cover,
/// arcing fire lobs over both, and line weapons target straight rays.
/// </summary>
public class WeaponAbilityRange : ConstantAbilityRange
{
    private GearData gear;

    public override List<Tile> GetTilesInRange(Board board)
    {
        Refresh();

        if (gear != null && gear.shape == WeaponShape.Line)
            return RayTiles(board);

        return base.GetTilesInRange(board);
    }

    /// <summary>
    /// Re-reads the worn weapon. Callers that read the horizontal/vertical
    /// fields directly (AI planning) call this first.
    /// </summary>
    public void Refresh()
    {
        gear = GearCatalog.EquippedWeapon(this);
        horizontal = gear != null && gear.range > 1 ? gear.range : 1;

        // Melee reaches one step up/down; ranged arcs over its distance
        vertical = Mathf.Max(1, horizontal);
    }

    // Direct fire is additionally stopped by standing units in the path
    protected override bool HasLineOfSight(Board board, Tile target)
    {
        var unitsBlock = gear == null || gear.arc == WeaponArc.Direct;
        return LineOfSight.Clear(board, unit.tile, target, unitsBlock);
    }

    // Line weapons aim down the four straight rays from the attacker
    private List<Tile> RayTiles(Board board)
    {
        var tiles = new List<Tile> { unit.tile };
        var directions = new[] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };

        foreach (var dir in directions)
        {
            for (var step = 1; step <= horizontal; step++)
            {
                var tile = board.GetTile(unit.tile.pos + new Point(dir.x * step, dir.y * step));
                if (tile == null)
                    break;

                if (Mathf.Abs(tile.height - unit.tile.height) > vertical)
                    continue;

                tiles.Add(tile);
            }
        }

        return tiles;
    }
}
