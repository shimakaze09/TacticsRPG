using System;

/// <summary>
/// Applies a named status effect to the target for a duration (names
/// resolve through the typed StatusRegistry).
/// </summary>
public class InflictAbilityEffect : BaseAbilityEffect
{
    public int duration;
    public string statusName;

    public override int Predict(Tile target)
    {
        return 0;
    }

    /// <summary>
    /// The duration this inflict would actually apply to the target after
    /// the ControlBudget contract — per-status and global caps, and zero for
    /// a boss-tier target immune to this status. Forecast UI and the AI read
    /// the same number the application path enforces (issue #57).
    /// </summary>
    public int ForecastDuration(Unit target)
    {
        if (!ControlBudget.TryGetProfile(statusName, out var profile))
            return ControlBudget.IsControl(statusName)
                ? UnityEngine.Mathf.Min(duration, ControlBudget.MaxControlDuration)
                : duration;

        if (profile.BossImmune && ControlBudget.IsBossTier(target))
            return 0;

        return UnityEngine.Mathf.Min(duration,
            UnityEngine.Mathf.Min(profile.MaxDuration, ControlBudget.MaxControlDuration));
    }

    protected override int OnApply(Tile target)
    {
        var unit = target.content.GetComponent<Unit>();
        StatusRegistry.Inflict(unit, statusName, duration);
        return 0;
    }

    // Kept as the AI's lookup point for whether a target already carries a
    // status before re-applying it; backed by the registry.
    public static Type ResolveStatusType(string name)
    {
        return StatusRegistry.Resolve(name);
    }
}