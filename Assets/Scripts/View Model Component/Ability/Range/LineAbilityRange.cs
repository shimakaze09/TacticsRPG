using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineAbilityRange : AbilityRange
{
    public override bool directionOriented => true;

    public override List<Tile> GetTilesInRange(Board board)
    {
        var startPos = unit.tile.pos;
        Point endPos;
        var retValue = new List<Tile>();

        switch (unit.dir)
        {
            case Directions.North:
                endPos = new Point(startPos.x, board.max.y);
                break;
            case Directions.East:
                endPos = new Point(board.max.x, startPos.y);
                break;
            case Directions.South:
                endPos = new Point(startPos.x, board.min.y);
                break;
            default: // West
                endPos = new Point(board.min.x, startPos.y);
                break;
        }

        var dist = 0;
        while (startPos != endPos)
        {
            if (startPos.x < endPos.x) startPos.x++;
            else if (startPos.x > endPos.x) startPos.x--;

            if (startPos.y < endPos.y) startPos.y++;
            else if (startPos.y > endPos.y) startPos.y--;

            var t = board.GetTile(startPos);
            if (t != null && Mathf.Abs(t.height - unit.tile.height) <= vertical)
                retValue.Add(t);

            dist++;
            if (dist >= horizontal)
                break;
        }

        return retValue;
    }
}