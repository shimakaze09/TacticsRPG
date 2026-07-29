using System.Collections.Generic;

/// <summary>
/// Single-tile area: the ability affects exactly the targeted tile.
/// </summary>
public class UnitAbilityArea : AbilityArea
{
    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        var retValue = new List<Tile>();
        var tile = board.GetTile(pos);
        if (tile != null)
            retValue.Add(tile);
        return retValue;
    }
}