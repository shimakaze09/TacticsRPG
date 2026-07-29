using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Standard range: all tiles within horizontal/vertical distance of the caster.
/// </summary>
public class ConstantAbilityRange : AbilityRange
{
    public override List<Tile> GetTilesInRange(Board board)
    {
        return board.Search(unit.tile, ExpandSearch);
    }

    private bool ExpandSearch(Tile from, Tile to)
    {
        return from.distance + 1 <= horizontal && Mathf.Abs(to.height - unit.tile.height) <= vertical;
    }
}