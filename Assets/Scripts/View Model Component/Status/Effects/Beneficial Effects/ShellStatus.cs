using UnityEngine;

/// <summary>
/// Shell: Magical attacks against this unit have their effective magical attack reduced by 1/3.
/// Affects damage of spells and accuracy of status spells. Lasts for 32 clock ticks (~3-4 turns).
/// </summary>
public class ShellStatus : TurnBasedStatusEffect
{
    [Tooltip("Magical defense multiplier (0.667 = reduce damage by 1/3)")]
    public float resistMultiplier = 0.667f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        // In FFT, Shell lasts 32 clock ticks
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
        // Boost magical defense/resistance
        if (e.StatType == StatTypes.RES)
        {
            // Increase resistance by ~50% (equivalent to reducing magic attack by 1/3)
            var modifier = new MultValueModifier(0, 1.5f);
            e.Exception.AddModifier(modifier);
        }
    }
}
