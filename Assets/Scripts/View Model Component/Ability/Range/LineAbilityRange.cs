using System.Collections.Generic;
using UnityEngine;

public class LineAbilityRange : AbilityRange
{
    public override bool directionOriented => true;

    public override List<Tile> GetTilesInRange(Board board)
    {
        var startPos = unit.tile.pos;
        var retValue = new List<Tile>();

        var endPos = unit.dir switch
        {
            Directions.North => new Point(startPos.x, board.max.y),
            Directions.East => new Point(board.max.x, startPos.y),
            Directions.South => new Point(startPos.x, board.min.y),
            _ => new Point(board.min.x, startPos.y)
        };

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