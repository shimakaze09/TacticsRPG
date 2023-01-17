using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBack : Movement
{
    protected override bool ExpandSearch(Tile from, Tile to)
    {
        if (to.height > from.height)
            return false;

        return base.ExpandSearch(from, to);
    }

    public override IEnumerator Traverse(Tile tile)
    {
        unit.Place(tile);

        var targets = new List<Tile>();
        while (tile != null)
        {
            targets.Insert(0, tile);
            tile = tile.prev;
        }

        for (var i = 1; i < targets.Count; i++)
        {
            var from = targets[i - 1];
            var to = targets[i];

            if (from.height == to.height)
                yield return StartCoroutine(Knock(to));
            else
                yield return StartCoroutine(Fall(to));
        }

        yield return null;
    }

    private IEnumerator Knock(Tile target)
    {
        var tweener = transform.MoveTo(target.center, 0.1f, EasingEquations.Linear);
        while (tweener != null)
            yield return null;
    }

    private IEnumerator Fall(Tile to)
    {
        var tweener = transform.MoveTo(to.center, 0.5f, EasingEquations.Linear);

        var t2 = jumper.MoveToLocal(new Vector3(0, Tile.stepHeight * 2f, 0), tweener.duration / 5f,
            EasingEquations.Linear);
        t2.loopCount = 1;
        t2.loopType = EasingControl.LoopType.PingPong;

        while (tweener != null)
            yield return null;
    }
}