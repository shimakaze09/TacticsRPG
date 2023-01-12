﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiniteAbilityRange : AbilityRange
{
    public override bool positionOriented => false;

    public override List<Tile> GetTilesInRange(Board board)
    {
        return new List<Tile>(board.tiles.Values);
    }
}