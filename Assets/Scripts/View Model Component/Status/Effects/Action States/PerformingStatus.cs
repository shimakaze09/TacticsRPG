using UnityEngine;

/// <summary>
/// The unit is reciting poetry or dancing (Bardsong/Dance).
/// Cannot evade attacks. Only manual cancellation ends this status.
/// </summary>
public class PerformingStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to evasion checks - performing units cannot evade
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
        // Reduce evasion to 0 while performing
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }
    }
}
