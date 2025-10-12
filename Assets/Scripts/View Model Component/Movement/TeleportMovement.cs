﻿using UnityEngine;
using System.Collections;

public class TeleportMovement : Movement
{
    protected override TileTraversalFlags TraversalMask => TileTraversalFlags.Teleport;

    public override IEnumerator Traverse(Tile tile)
    {
        unit.Place(tile);

        var spin = jumper.RotateToLocal(new Vector3(0, 360, 0), 0.5f, EasingEquations.EaseInOutQuad);
        spin.loopCount = 1;
        spin.loopType = EasingControl.LoopType.PingPong;

        var shrink = transform.ScaleTo(Vector3.zero, 0.5f, EasingEquations.EaseInBack);

        while (shrink != null)
            yield return null;

        transform.position = tile.center;

        var grow = transform.ScaleTo(Vector3.one, 0.5f, EasingEquations.EaseOutBack);
        while (grow != null)
            yield return null;
    }
}