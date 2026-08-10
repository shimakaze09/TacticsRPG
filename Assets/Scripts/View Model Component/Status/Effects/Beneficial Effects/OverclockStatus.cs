using UnityEngine;

/// <summary>
/// Overclock (haste): Unit gains 50% more CT each clock tick, allowing them to
/// act more often. Does not directly affect the Speed stat. The multiplier is
/// data-configurable and clamped to the StatLimits CT-gain range (issue #19).
/// Opposed to Throttle.
/// </summary>
public class OverclockStatus : StatusEffect
{
    [Tooltip("CT gain multiplier (1.5 = 50% more CT per tick). Clamped to the StatLimits CT-gain range.")]
    public float ctMultiplier = 1.5f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, stats);
        }
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    // Scales every CTR gain by the configured multiplier so tooltip, forecast,
    // and actual initiative agree on one value
    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType == StatTypes.CTR)
        {
            var safe = Mathf.Clamp(ctMultiplier,
                StatLimits.MinCTGainMultiplier, StatLimits.MaxCTGainMultiplier);
            e.Exception.AddModifier(new MultDeltaModifier(0, safe));
        }
    }
}
