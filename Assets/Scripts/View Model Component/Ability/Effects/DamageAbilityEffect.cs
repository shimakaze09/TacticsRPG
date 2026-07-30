using UnityEngine;

/// <summary>
/// Deals damage: max(ATK x power/100 - DEF/2, 1), tweaked by
/// statuses/difficulty, clamped to StatLimits, with +-10% variance.
/// </summary>
public class DamageAbilityEffect : BaseAbilityEffect
{
    #region Public

    public override int Predict(Tile target)
    {
        var attacker = GetComponentInParent<Unit>();
        var defender = target.content.GetComponent<Unit>();

        // Get the attackers base attack stat considering
        // mission items, support check, status check, and equipment, etc
        var attack = GetStat(attacker, defender, typeof(GetAttackStatEvent), 0);

        // Get the targets base defense stat considering
        // mission items, support check, status check, and equipment, etc
        var defense = GetStat(attacker, defender, typeof(GetDefenseStatEvent), 0);

        // Get the abilities power stat considering possible variations
        var power = GetStat(attacker, defender, typeof(GetPowerEvent), 0);

        // Damage scales multiplicatively with the attack stat (power 100 =
        // one ATK's worth), so gear/buffs that raise ATK flow through every
        // ability; defense mitigates flat. Crits/elements/multipliers belong
        // in the TweakDamageEvent stage below.
        var damage = attack * power / 100 - defense / 2;
        damage = Mathf.Max(damage, 1);

        // Tweak the damage based on a variety of other checks like
        // Elemental damage, Critical Hits, Damage multipliers, etc.
        damage = GetStat(attacker, defender, typeof(TweakDamageEvent), damage);

        // Clamp the damage to a range
        damage = Mathf.Clamp(damage, minDamage, maxDamage);
        return -damage;
    }

    protected override int OnApply(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();

        // Start with the predicted damage value
        var value = Predict(target);

        // Add some random variance
        value = Mathf.FloorToInt(value * Random.Range(0.9f, 1.1f));

        // Critical hits roll here — at application, never in Predict, so
        // forecasts stay deterministic
        var attacker = GetComponentInParent<Unit>();
        if (CriticalHit.Roll(attacker))
        {
            value = Mathf.FloorToInt(value * CriticalHit.DamageMultiplier);
            this.Publish(new CriticalHitEvent(attacker, defender));
        }

        // Clamp the damage to a range
        value = Mathf.Clamp(value, minDamage, maxDamage);

        // Apply the damage to the target
        var s = defender.GetComponent<Stats>();
        s[StatTypes.HP] += value;
        return value;
    }

    #endregion
}