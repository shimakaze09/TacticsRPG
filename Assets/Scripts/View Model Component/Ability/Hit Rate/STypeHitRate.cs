using UnityEngine;

public class STypeHitRate : HitRate
{
    public override int Calculate(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        if (AutomaticMiss(defender))
            return Final(100);

        if (AutomaticHit(defender))
            return Final(0);

        var res = GetResistance(defender);
        res = AdjustForStatusEffects(defender, res);
        res = AdjustForRelativeFacing(defender, res);
        res = Mathf.Clamp(res, 0, 100);
        return Final(res);
    }

    private int GetResistance(Unit target)
    {
        var s = target.GetComponentInParent<Stats>();
        return s[StatTypes.RES];
    }

    private int AdjustForRelativeFacing(Unit target, int rate)
    {
        switch (attacker.GetFacing(target))
        {
            case Facings.Front:
                return rate;
            case Facings.Side:
                return rate - 10;
            default:
                return rate - 20;
        }
    }
}