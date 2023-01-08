using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FacingsExtensions
{
    public static Facings GetFacing(this Unit attacker, Unit target)
    {
        Vector2 targetDirection = target.dir.GetNormal();
        var approachDirection = ((Vector2)(target.tile.pos - attacker.tile.pos)).normalized;
        var dot = Vector2.Dot(approachDirection, targetDirection);
        return dot switch
        {
            >= 0.45f => Facings.Back,
            <= -0.45f => Facings.Front,
            _ => Facings.Side
        };
    }
}