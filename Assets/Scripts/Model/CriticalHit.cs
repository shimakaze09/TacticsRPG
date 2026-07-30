using UnityEngine;

/// <summary>
/// Critical hit law: rolled at application time (never in Predict, so
/// forecasts and AI stay deterministic), multiplying the final damage.
/// Chance = base 5% plus any CritBonus gear traits the attacker wears.
/// </summary>
public static class CriticalHit
{
    public const int BaseChancePercent = 5;
    public const float DamageMultiplier = 1.5f;

    /// <summary>The attacker's current crit chance in percent.</summary>
    public static int Chance(Unit attacker)
    {
        var chance = BaseChancePercent;

        var equipment = attacker != null ? attacker.GetComponent<Equipment>() : null;
        if (equipment != null)
        {
            foreach (var item in equipment.items)
            {
                var tag = item.GetComponent<GearTag>();
                var gear = tag != null ? GearCatalog.Get(tag.gearId) : null;
                if (gear == null || gear.traits == null)
                    continue;

                foreach (var trait in gear.traits)
                    if (trait.type == GearTraitType.CritBonus)
                        chance += trait.value;
            }
        }

        return Mathf.Clamp(chance, 0, 50);
    }

    public static bool Roll(Unit attacker)
    {
        return Random.Range(0, 100) < Chance(attacker);
    }
}
