using UnityEngine;

/// <summary>
/// Ghosted (invisibility): Attacks ignore target's evasion. Unit is ignored by AI.
/// Removed when unit performs an action (other than moving) or is attacked.
/// </summary>
public class GhostedStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    [Tooltip("Added to this unit's effective evade/resist while ghosted")]
    public int evasionBonus = 25;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to HP changes to detect being attacked
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
        }

        this.Subscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);

        this.Unsubscribe<HitRateStatusCheckEvent>(OnHitRateCheck);
    }

    private void OnHitRateCheck(HitRateStatusCheckEvent e)
    {
        if (e.Target != owner)
            return;

        e.Args.HitRate += evasionBonus;
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // If unit was attacked (HP changed), remove invisible
        if (e.NewValue != e.OldValue)
        {
            RemoveInvisible();
        }
    }

    private void RemoveInvisible()
    {
        var cond = GetComponentInChildren<StatusCondition>();
        if (cond != null)
            cond.Remove();
        else
            Destroy(this);
    }

    // This should be called by the combat system when unit attacks
    public void OnPerformAction()
    {
        RemoveInvisible();
    }

    // This method should be checked by the combat system
    // to determine if evasion should be ignored
    public bool IgnoresTargetEvasion()
    {
        return true;
    }
}
