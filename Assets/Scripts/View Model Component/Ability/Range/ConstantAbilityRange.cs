using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Standard range: all tiles within horizontal/vertical distance of the caster.
/// Ranged versions (horizontal > 1) additionally require line of sight, so
/// terrain provides real cover against archery and protocol fire.
/// </summary>
public class ConstantAbilityRange : AbilityRange
{
    [Tooltip("When true (default), tiles hidden behind terrain cannot be targeted")]
    public bool requiresLineOfSight = true;

    /// <summary>Collects tiles in range, then drops any without line of sight.</summary>
    public override List<Tile> GetTilesInRange(Board board)
    {
        var tiles = board.Search(unit.tile, ExpandSearch);

        // Melee (range 1) has no intermediate tiles to block it
        if (requiresLineOfSight && horizontal > 1)
            tiles.RemoveAll(t => !HasLineOfSight(board, t));

        return tiles;
    }

    // Subclasses may tighten the sight rule (e.g. direct fire blocked by units)
    protected virtual bool HasLineOfSight(Board board, Tile target)
    {
        return LineOfSight.Clear(board, unit.tile, target);
    }

    // Search filter: within horizontal steps and vertical height difference
    private bool ExpandSearch(Tile from, Tile to)
    {
        return from.distance + 1 <= horizontal && Mathf.Abs(to.height - unit.tile.height) <= vertical;
    }
}
