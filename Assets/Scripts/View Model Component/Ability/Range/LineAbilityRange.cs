using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Straight line of tiles in the caster's facing direction. The line is
/// stopped by terrain rising above the caster's vertical tolerance — walls
/// end a volley rather than letting it pass through.
/// </summary>
public class LineAbilityRange : AbilityRange
{
    public override bool directionOriented => true;

    /// <summary>Walks tile by tile in the facing direction until range or a wall ends the line.</summary>
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
            if (t != null)
            {
                var rise = t.height - unit.tile.height;

                // Terrain above the tolerance blocks everything past it
                if (rise > vertical)
                    break;

                // Deep drops can't be hit but the line continues over them
                if (Mathf.Abs(rise) <= vertical)
                    retValue.Add(t);
            }

            dist++;
            if (dist >= horizontal)
                break;
        }

        return retValue;
    }
}
