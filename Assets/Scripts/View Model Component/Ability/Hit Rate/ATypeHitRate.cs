using UnityEngine;
using System.Collections;

public class ATypeHitRate : HitRate
{
    public override int Calculate(Unit attacker, Unit target)
    {
        if (AutomaticHit(attacker, target))
            return Final(0);
        if (AutomaticMiss(attacker, target))
            return Final(100);
        var evade = GetEvade(target);
        evade = AdjustForRelativeFacing(attacker, target, evade);
        evade = AdjustForStatusEffects(attacker, target, evade);
        evade = Mathf.Clamp(evade, 5, 95);
        return Final(evade);
    }

    private int GetEvade(Unit target)
    {
        var s = target.GetComponentInParent<Stats>();
        return Mathf.Clamp(s[StatTypes.EVD], 0, 100);
    }

    private int AdjustForRelativeFacing(Unit attacker, Unit target, int rate)
    {
        return attacker.GetFacing(target) switch
        {
            Facings.Front => rate,
            Facings.Side => rate / 2,
            _ => rate / 4
        };
    }
}