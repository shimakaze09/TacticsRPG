using UnityEngine;

/// <summary>
/// Condition that removes the status when the unit takes damage.
/// Used by Sleep, Confuse, and similar statuses.
/// </summary>
public class DamageRemovalCondition : StatusCondition
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats != null)
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        // Remove status if unit took HP damage
        if (e.StatType == StatTypes.HP && e.NewValue < e.OldValue)
        {
            Remove();
        }
    }
}
