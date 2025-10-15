using UnityEngine;

/// <summary>
/// Protect: Physical attacks against this unit have their effective physical attack reduced by 1/3.
/// Lasts for 32 clock ticks (~3-4 turns).
/// </summary>
public class ProtectStatus : TurnBasedStatusEffect
{
    [Tooltip("Physical defense multiplier (0.667 = reduce damage by 1/3)")]
    public float defenseMultiplier = 0.667f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        // In FFT, Protect lasts 32 clock ticks
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
        // Boost physical defense
        if (e.StatType == StatTypes.DEF)
        {
            // Increase defense by ~50% (equivalent to reducing attack by 1/3)
            var modifier = new MultValueModifier(0, 1.5f);
            e.Exception.AddModifier(modifier);
        }
    }
}
