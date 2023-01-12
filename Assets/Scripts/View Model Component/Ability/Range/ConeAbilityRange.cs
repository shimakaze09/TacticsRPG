using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConeAbilityRange : AbilityRange
{
    public override bool directionOriented => true;

    public override List<Tile> GetTilesInRange(Board board)
    {
        var pos = unit.tile.pos;
        var retValue = new List<Tile>();
        var dir = unit.dir == Directions.North || unit.dir == Directions.East ? 1 : -1;
        var lateral = 1;

        if (unit.dir == Directions.North || unit.dir == Directions.South)
            for (var y = 1; y <= horizontal; ++y)
            {
                var min = -(lateral / 2);
                var max = lateral / 2;
                for (var x = min; x <= max; ++x)
                {
                    var next = new Point(pos.x + x, pos.y + y * dir);
                    var tile = board.GetTile(next);
                    if (ValidTile(tile))
                        retValue.Add(tile);
                }

                lateral += 2;
            }
        else
            for (var x = 1; x <= horizontal; ++x)
            {
                var min = -(lateral / 2);
                var max = lateral / 2;
                for (var y = min; y <= max; ++y)
                {
                    var next = new Point(pos.x + x * dir, pos.y + y);
                    var tile = board.GetTile(next);
                    if (ValidTile(tile))
                        retValue.Add(tile);
                }

                lateral += 2;
            }

        return retValue;
    }

    private bool ValidTile(Tile t)
    {
        return t != null && Mathf.Abs(t.height - unit.tile.height) <= vertical;
    }
}