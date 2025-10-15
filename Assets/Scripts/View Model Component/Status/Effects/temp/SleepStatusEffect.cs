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
            this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck);

        if (stats)
            this.AddObserver(OnHitNotification, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck);
        this.RemoveObserver(OnHitNotification, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnCanPerformCheck(object sender, object args)
    {
        var unit = (sender as Ability).GetComponentInParent<Unit>();
        if (owner == unit)
        {
            var exc = args as BaseException;
            if (exc.defaultToggle)
                exc.FlipToggle(); // prevent performing ability while asleep
        }
    }

    private void OnHitNotification(object sender, object args)
    {
        // If unit takes damage, wake up (remove status)
        var vce = args as ValueChangeException;
        if (vce == null)
            return;

        if (vce.GetModifiedValue() < vce.fromValue)
        {
            // HP decreased -> remove sleep
            var cond = GetComponentInChildren<StatusCondition>();
            if (cond != null)
                cond.Remove();
        }
    }
}