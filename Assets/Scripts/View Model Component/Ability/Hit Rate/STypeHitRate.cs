using UnityEngine;

/// <summary>
/// Status-type accuracy: reduced by target resistance (RES growth per
/// ProgressionModel, Steeled stacks, facing), then bounded by the
/// ControlBudget contract — outside auto-hit/miss exceptions, a status
/// attempt is never a certainty and never impossible (issue #57).
/// </summary>
public class STypeHitRate : HitRate
{
    public override int Calculate(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        if (AutomaticMiss(defender))
            return Final(100);

        // Boss policy (issue #57): a boss-immune status can never land on a
        // boss-tier target — the forecast says so honestly
        var inflict = GetComponent<InflictAbilityEffect>();
        var profile = default(ControlBudget.ControlProfile);
        var hasProfile = inflict != null && ControlBudget.TryGetProfile(inflict.statusName, out profile);
        if (hasProfile && profile.BossImmune && ControlBudget.IsBossTier(defender))
            return 0;

        if (AutomaticHit(defender))
            return Final(0);

        var res = GetResistance(defender);
        res = AdjustForStatusEffects(defender, res);
        res = AdjustForRelativeFacing(defender, res);
        res = Mathf.Clamp(res, 0, 100);
        var chance = Mathf.Clamp(Final(res), ControlBudget.MinChance, ControlBudget.MaxChance);

        // Per-status accuracy ceiling: stronger denial caps lower — enforced
        // here so data, forecast, and the actual roll all agree
        if (hasProfile)
            chance = Mathf.Min(chance, profile.MaxAccuracy);

        return chance;
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