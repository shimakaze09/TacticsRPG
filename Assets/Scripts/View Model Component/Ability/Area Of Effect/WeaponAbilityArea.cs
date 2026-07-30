using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Area for basic weapon attacks, following the weapon's shape: Target hits
/// the chosen tile; Line sprays from the attacker through it to full reach;
/// Sweep swings through the target tile and the two tiles flanking it
/// (perpendicular to the strike). Wide shapes pay for coverage through
/// GearData.damagePercent (applied by WeaponPowerScale).
/// </summary>
public class WeaponAbilityArea : AbilityArea
{
    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        var gear = GearCatalog.EquippedWeapon(this);
        var unit = GetComponentInParent<Unit>();

        if (gear != null && unit != null && unit.tile != null)
        {
            if (gear.shape == WeaponShape.Line)
                return LineTiles(board, unit.tile.pos, pos, gear.range);
            if (gear.shape == WeaponShape.Sweep)
                return SweepTiles(board, unit.tile.pos, pos);
        }

        var tiles = new List<Tile>();
        var single = board.GetTile(pos);
        if (single != null)
            tiles.Add(single);
        return tiles;
    }

    // Spray from the attacker through the chosen tile out to full reach
    private static List<Tile> LineTiles(Board board, Point origin, Point pos, int range)
    {
        var tiles = new List<Tile>();
        var dir = StepToward(origin, pos);
        var reach = Mathf.Max(1, range);
        for (var step = 1; step <= reach; step++)
        {
            var tile = board.GetTile(origin + new Point(dir.x * step, dir.y * step));
            if (tile != null)
                tiles.Add(tile);
        }

        return tiles;
    }

    // The target tile plus its two neighbors perpendicular to the strike
    private static List<Tile> SweepTiles(Board board, Point origin, Point pos)
    {
        var tiles = new List<Tile>();
        var dir = StepToward(origin, pos);
        var perpendicular = new Point(dir.y, dir.x);

        foreach (var p in new[] { pos, pos + perpendicular, pos - perpendicular })
        {
            var tile = board.GetTile(p);
            if (tile != null)
                tiles.Add(tile);
        }

        return tiles;
    }

    // Unit step along the dominant axis from origin toward the chosen tile
    private static Point StepToward(Point origin, Point pos)
    {
        var delta = pos - origin;
        return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
            ? new Point(delta.x >= 0 ? 1 : -1, 0)
            : new Point(0, delta.y >= 0 ? 1 : -1);
    }
}
