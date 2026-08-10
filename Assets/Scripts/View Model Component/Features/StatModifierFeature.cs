using UnityEngine;

/// <summary>
/// Feature that adjusts a stat while active (weapon ATK bonuses, armor DEF).
/// Applies cap-aware: the write clamps to the stat's StatLimits ceiling and
/// remembers the delta that actually landed, so removing the item restores
/// exactly the prior value — a bonus clipped at a cap must never underflow
/// the base stat on unequip (issue #57 review). This keeps the live
/// equip/unequip path convergent with JobManager.RecalculateStats.
/// </summary>
public class StatModifierFeature : Feature
{
    #region Fields / Properties

    public StatTypes type;
    public int amount;

    // The clamped change OnApply actually made; OnRemove reverses this, not
    // the nominal amount
    private int appliedDelta;

    private Stats stats => _target.GetComponentInParent<Stats>();

    #endregion

    #region Protected

    protected override void OnApply()
    {
        int before = stats[type];
        int after = Mathf.Clamp(before + amount, 0, CapFor(type));
        appliedDelta = after - before;
        stats[type] = after;
    }

    protected override void OnRemove()
    {
        stats[type] -= appliedDelta;
        appliedDelta = 0;
    }

    #endregion

    #region Private

    // Ceiling for each capped stat; uncapped stats pass through unchanged
    private static int CapFor(StatTypes type)
    {
        switch (type)
        {
            case StatTypes.MHP: return StatLimits.MaxHP;
            case StatTypes.MMP: return StatLimits.MaxMP;
            case StatTypes.RES: return StatLimits.MaxRES;
            case StatTypes.ATK:
            case StatTypes.DEF:
            case StatTypes.MAT:
            case StatTypes.MDF:
            case StatTypes.SPD: return StatLimits.MaxPrimaryStat;
            default: return int.MaxValue;
        }
    }

    #endregion
}
