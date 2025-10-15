using UnityEngine;


public class SleepStatusEffect : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner)
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);

        if (stats)
            this.SubscribeToSender<StatDidChangeEvent>(OnHitNotification, stats);
    }

    private void OnDisable()
    {
        this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnHitNotification, stats);
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (owner == unit)
        {
            if (e.Exception.defaultToggle)
                e.Exception.FlipToggle(); // prevent performing ability while asleep
        }
    }

    private void OnHitNotification(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // If unit takes damage, wake up (remove status)
        if (e.NewValue < e.OldValue)
        {
            // HP decreased -> remove sleep
            var cond = GetComponentInChildren<StatusCondition>();
            if (cond != null)
                cond.Remove();
        }
    }
}