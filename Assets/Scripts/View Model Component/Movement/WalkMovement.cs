using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground movement: steps tile to tile, turning at corners and hopping height
/// changes.
/// </summary>
public class WalkMovement : Movement
{
    #region Protected

    protected override bool ExpandSearch(Tile from, Tile to)
    {
        // Skip if the distance in height between the two tiles is more than the unit can jump
        if (Mathf.Abs(from.height - to.height) > jumpHeight)
            return false;

        // Occupied tiles block movement — except downed units, which can be
        // stepped over (though never ended on; Filter removes occupied tiles)
        if (to.content != null && !IsDownedOccupant(to))
            return false;

        return base.ExpandSearch(from, to);
    }

    // True when the tile's occupant is KO'd and therefore passable
    private static bool IsDownedOccupant(Tile tile)
    {
        return tile.content.GetComponentInChildren<KOStatus>() != null;
    }

    public override IEnumerator Traverse(Tile tile)
    {
        unit.Place(tile);

        // Build a list of way points from the unit's 
        // starting tile to the destination tile
        var targets = new List<Tile>();
        while (tile != null)
        {
            targets.Insert(0, tile);
            tile = tile.prev;
        }

        // Move to each way point in succession
        for (var i = 1; i < targets.Count; i++)
        {
            var from = targets[i - 1];
            var to = targets[i];

            var dir = from.GetDirection(to);
            if (unit.dir != dir)
                yield return StartCoroutine(Turn(dir));

            if (from.height == to.height)
                yield return StartCoroutine(Walk(to));
            else
                yield return StartCoroutine(Jump(to));
        }

        yield return null;
    }

    #endregion

    #region Private

    private IEnumerator Walk(Tile target)
    {
        var tweener = transform.MoveTo(target.center, 0.5f, EasingEquations.Linear);
        while (tweener != null)
            yield return null;
    }

    private IEnumerator Jump(Tile to)
    {
        var tweener = transform.MoveTo(to.center, 0.5f, EasingEquations.Linear);

        var t2 = jumper.MoveToLocal(new Vector3(0, Tile.stepHeight * 2f, 0), tweener.duration / 2f,
            EasingEquations.EaseOutQuad);
        t2.loopCount = 1;
        t2.loopType = EasingControl.LoopType.PingPong;

        while (tweener != null)
            yield return null;
    }

    #endregion
}