using UnityEngine;

public abstract class DamageRemovableStatusEffect : StatusEffect
{
    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();
        if (stats != null)
            this.AddObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.RemoveObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.HP), stats);
    }

    private void OnStatsChanged(object sender, object args)
    {
        var vce = args as ValueChangeException;
        if (vce == null)
            return;

        // If HP decreased, clear the status
        if (vce.GetModifiedValue() < vce.fromValue)
        {
            var cond = GetComponentInChildren<StatusCondition>() ?? GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }
}