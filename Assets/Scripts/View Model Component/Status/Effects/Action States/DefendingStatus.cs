using UnityEngine;

/// <summary>
/// The unit has assumed a defensive stance, doubling their evasion stats.
/// Lasts until they take another turn.
/// </summary>
public class DefendingStatus : StatusEffect
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to stat changes to double evasion
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
        // Double evasion stats while defending
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 2f);
            e.Exception.AddModifier(modifier);
        }
    }
}
