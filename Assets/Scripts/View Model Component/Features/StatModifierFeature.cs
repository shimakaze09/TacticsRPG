using UnityEngine;

/// <summary>
/// Feature that adjusts a stat while active (weapon ATK bonuses, armor DEF).
/// On units with a JobManager, apply and remove both defer to
/// JobManager.RecalculateStats — Equipment updates its item list before the
/// features run, so one deterministic recomputation lands or removes the
/// bonus with caps applied and nothing stored to go stale (a recorded delta
/// would drift whenever the derived baseline changed while equipped —
/// issue #57 review). Targets without a job system fall back to a cap-aware,
/// exactly-reversible direct write.
/// </summary>
public class StatModifierFeature : Feature
{
    #region Fields / Properties

    public StatTypes type;
    public int amount;

    // Fallback-path memory: the clamped change OnApply actually made, so
    // OnRemove reverses exactly that rather than the nominal amount
    private int appliedDelta;

    private Stats stats => _target.GetComponentInParent<Stats>();

    #endregion

    #region Protected

    protected override void OnApply()
    {
        var jobManager = _target.GetComponentInParent<JobManager>();
        if (jobManager != null)
        {
            jobManager.RecalculateStats();
            return;
        }

        int before = stats[type];
        int after = Mathf.Clamp(before + amount, 0, CapFor(type));
        appliedDelta = after - before;
        stats[type] = after;
    }

    protected override void OnRemove()
    {
        var jobManager = _target.GetComponentInParent<JobManager>();
        if (jobManager != null)
        {
            jobManager.RecalculateStats();
            return;
        }

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
