using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STypeHitRate : HitRate
{
    public override int Calculate(Unit attacker, Unit target)
    {
        if (AutomaticMiss(attacker, target))
            return Final(100);
        if (AutomaticHit(attacker, target))
            return Final(0);

        var res = GetResistance(target);
        res = AdjustForStatusEffects(attacker, target, res);
        res = AdjustForRelativeFacing(attacker, target, res);
        res = Mathf.Clamp(res, 0, 100);
        return Final(res);
    }

    private int GetResistance(Unit target)
    {
        var s = target.GetComponentInParent<Stats>();
        return s[StatTypes.RES];
    }

    private int AdjustForRelativeFacing(Unit attacker, Unit target, int rate)
    {
        return attacker.GetFacing(target) switch
        {
            Facings.Front => rate,
            Facings.Side => rate - 10,
            _ => rate - 20
        };
    }
}