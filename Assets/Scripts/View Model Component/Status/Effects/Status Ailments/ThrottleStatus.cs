using UnityEngine;

/// <summary>
/// Throttle (slow): Unit gains half CT each clock tick and its effective Speed
/// is halved (round down). The CT multiplier is data-configurable and clamped
/// to the StatLimits CT-gain range (issue #19). Opposed to Overclock.
/// </summary>
public class ThrottleStatus : StatusEffect
{
    [Tooltip("CT gain multiplier (0.5 = half CT per tick). Clamped to the StatLimits CT-gain range.")]
    public float ctMultiplier = 0.5f;

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

    // Scales CTR gains by the clamped multiplier and halves effective Speed so
    // both the initiative math and any SPD-derived checks see the slow
    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType == StatTypes.CTR)
        {
            var safe = Mathf.Clamp(ctMultiplier,
                StatLimits.MinCTGainMultiplier, StatLimits.MaxCTGainMultiplier);
            e.Exception.AddModifier(new MultDeltaModifier(0, safe));
        }

        if (e.StatType == StatTypes.SPD)
        {
            e.Exception.AddModifier(new MultValueModifier(0, 0.5f));
        }
    }
}
