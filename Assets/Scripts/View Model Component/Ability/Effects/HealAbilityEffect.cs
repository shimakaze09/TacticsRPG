using UnityEngine;

public class HealAbilityEffect : BaseAbilityEffect
{
    public override int Predict(Tile target)
    {
        var attacker = GetComponentInParent<Unit>();
        var defender = target.content.GetComponent<Unit>();
        return GetStat(attacker, defender, typeof(GetPowerEvent), 0);
    }

    protected override int OnApply(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();

        // Start with the predicted value
        var value = Predict(target);

        // Add some random variance
        value = Mathf.FloorToInt(value * Random.Range(0.9f, 1.1f));

        // Clamp the amount to a range
        value = Mathf.Clamp(value, minDamage, maxDamage);

        // Apply the amount to the target
        var s = defender.GetComponent<Stats>();
        s[StatTypes.HP] += value;
        return value;
    }
}