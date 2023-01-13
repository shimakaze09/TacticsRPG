using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelfAbilityRange : AbilityRange
{
    public override bool positionOriented => false;

    public override List<Tile> GetTilesInRange(Board board)
    {
        var retValue = new List<Tile>(1) { unit.tile };
        return retValue;
    }
}