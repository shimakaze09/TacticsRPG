using UnityEngine;

public abstract class DamageRemovableStatusEffect : StatusEffect
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats != null)
            this.SubscribeToSender<StatDidChangeEvent>(OnStatsChanged, stats);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatsChanged, stats);
    }

    private void OnStatsChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // If HP decreased, clear the status
        if (e.NewValue < e.OldValue)
        {
            var cond = GetComponentInChildren<StatusCondition>() ?? GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }
}