using System.Collections.Generic;
using UnityEngine;

public class SpecifyAbilityArea : AbilityArea
{
    public int horizontal;
    private Tile tile;
    public int vertical;

    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        tile = board.GetTile(pos);
        return board.Search(tile, ExpandSearch);
    }

    private bool ExpandSearch(Tile from, Tile to)
    {
        return from.distance + 1 <= horizontal && Mathf.Abs(to.height - tile.height) <= vertical;
    }
}