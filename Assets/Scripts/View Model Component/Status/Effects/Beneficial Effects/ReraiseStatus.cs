using UnityEngine;

/// <summary>
/// Reraise: When KO'd at 100 CT, revive with 10% max HP instead of decreasing death counter.
/// Does not naturally wear off. Nullifies Doom if present.
/// </summary>
public class ReraiseStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    [Tooltip("Percentage of max HP to restore on revival (0.1 = 10%)")]
    public float reviveHPPercent = 0.1f;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to HP changes to detect KO
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
        }
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // Check if unit was just KO'd (HP reached 0)
        if (e.NewValue <= 0 && e.OldValue > 0)
        {
            // Check if it's the unit's active turn (CT = 100)
            // This would need integration with your turn system
            // For now, we'll trigger the revival
            TriggerRevival();
        }
    }

    private void TriggerRevival()
    {
        if (stats == null)
            return;

        // Revive with 10% of max HP (rounded up)
        int maxHP = stats[StatTypes.MHP];
        int reviveHP = Mathf.CeilToInt(maxHP * reviveHPPercent);
        stats.SetValue(StatTypes.HP, reviveHP, false);

        // Remove Reraise after use
        var cond = GetComponent<StatusCondition>();
        if (cond != null)
            cond.Remove();
        else
            Destroy(this);
    }
}
