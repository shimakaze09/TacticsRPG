using UnityEngine;

/// <summary>
/// Slow: Unit's effective speed is halved (round down).
/// Lasts for 32 ticks (~3-4 turns). Opposed to Haste.
/// </summary>
public class SlowStatus : StatusEffect
{
    [Tooltip("CT gain multiplier (0.5 = half CT gain)")]
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

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Halve CT gain (if your system uses CTR or similar stat)
        if (e.StatType == StatTypes.CTR)
        {
            // Half the CT increment
            var modifier = new MultDeltaModifier(0, ctMultiplier);
            e.Exception.AddModifier(modifier);
        }

        // Also halve speed stat
        if (e.StatType == StatTypes.SPD)
        {
            var modifier = new MultValueModifier(0, 0.5f);
            e.Exception.AddModifier(modifier);
        }
    }
}
