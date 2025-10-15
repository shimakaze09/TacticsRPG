using UnityEngine;

/// <summary>
/// Haste: Unit gains 50% more CT each clock tick, allowing them to act more often.
/// Does not directly affect Speed stat. Lasts for 32 clock ticks (~3-4 turns).
/// Opposed to Slow.
/// </summary>
public class HasteStatus : TurnBasedStatusEffect
{
    [Tooltip("CT gain multiplier (2.0 = double CT gain = 50% more)")]
    public float ctMultiplier = 1.5f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        // In FFT, Haste lasts 32 clock ticks
        remainingTurns = 4;

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

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Increase CT gain (if your system uses CTR or similar stat)
        if (e.StatType == StatTypes.CTR)
        {
            // Double the CT increment
            var modifier = new MultDeltaModifier(0, 2);
            e.Exception.AddModifier(modifier);
        }
    }
}
