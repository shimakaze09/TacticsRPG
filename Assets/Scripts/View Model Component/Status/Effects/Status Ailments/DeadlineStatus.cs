using UnityEngine;

/// <summary>
/// Deadline (delayed KO): A countdown appears starting at 3. Unit is KO'd when it reaches the fourth active turn.
/// Negated by Failsafe or instant death protection. If undead, Deadline is lifted at counter zero.
/// </summary>
public class DeadlineStatus : StatusEffect
{
    [Tooltip("Number of active turns before KO")]
    public int doomCounter = 3;

    private Unit owner;
    private Stats stats;
    private int turnCount = 0;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner != null)
        {
            this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, owner);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, owner);
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
        turnCount++;

        if (turnCount >= doomCounter)
        {
            // Check for Failsafe — a sibling status effect under the same
            // Status root, never on this effect's own GameObject
            var statusRoot = GetComponentInParent<Status>();
            var reraise = statusRoot != null ? statusRoot.GetComponentInChildren<FailsafeStatus>() : null;
            if (reraise != null)
            {
                // Failsafe and Doom nullify each other
                var raiseCond = reraise.GetComponentInChildren<StatusCondition>();
                if (raiseCond != null)
                    raiseCond.Remove();

                var doomCond = GetComponentInChildren<StatusCondition>();
                if (doomCond != null)
                    doomCond.Remove();

                return;
            }

            // Check for Revenant — also a sibling status effect
            var undead = statusRoot != null ? statusRoot.GetComponentInChildren<RevenantStatus>() : null;
            if (undead != null)
            {
                // For undead, Deadline is lifted instead of KO
                var cond = GetComponentInChildren<StatusCondition>();
                if (cond != null)
                    cond.Remove();
                else
                    Destroy(this);
                return;
            }

            // KO the unit
            if (stats != null)
            {
                stats.SetValue(StatTypes.HP, 0, false);
            }

            // Remove doom status
            var doomCondition = GetComponentInChildren<StatusCondition>();
            if (doomCondition != null)
                doomCondition.Remove();
            else
                Destroy(this);
        }
    }

    public int GetDoomCounter()
    {
        return Mathf.Max(0, doomCounter - turnCount);
    }
}
