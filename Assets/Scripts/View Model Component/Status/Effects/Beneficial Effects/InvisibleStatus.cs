using UnityEngine;

/// <summary>
/// Invisible: Attacks ignore target's evasion. Unit is ignored by AI.
/// Removed when unit performs an action (other than moving) or is attacked.
/// </summary>
public class InvisibleStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            // Subscribe to HP changes to detect being attacked
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

        // If unit was attacked (HP changed), remove invisible
        if (e.NewValue != e.OldValue)
        {
            RemoveInvisible();
        }
    }

    private void RemoveInvisible()
    {
        var cond = GetComponent<StatusCondition>();
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
