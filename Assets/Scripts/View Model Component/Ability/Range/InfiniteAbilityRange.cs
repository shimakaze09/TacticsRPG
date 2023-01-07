using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteAbilityRange : AbilityRange
{
    public override List<Tile> GetTilesInRange(Board board)
    {
        return new List<Tile>(board.tiles.Values);
    }
}