using UnityEngine;

public class ATypeHitRate : HitRate
{
    public override int Calculate(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        if (AutomaticHit(defender))
            return Final(0);

        if (AutomaticMiss(defender))
            return Final(100);

        var evade = GetEvade(defender);
        evade = AdjustForRelativeFacing(defender, evade);
        evade = AdjustForStatusEffects(defender, evade);
        evade = Mathf.Clamp(evade, 5, 95);
        return Final(evade);
    }

    private int GetEvade(Unit target)
    {
        var s = target.GetComponentInParent<Stats>();
        return Mathf.Clamp(s[StatTypes.EVD], 0, 100);
    }

    private int AdjustForRelativeFacing(Unit target, int rate)
    {
        return attacker.GetFacing(target) switch
        {
            Facings.Front => rate,
            Facings.Side => rate / 2,
            _ => rate / 4
        };
    }
}