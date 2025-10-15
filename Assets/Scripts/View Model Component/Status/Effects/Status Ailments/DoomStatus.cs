using UnityEngine;

/// <summary>
/// Doom: A countdown appears starting at 3. Unit is KO'd when it reaches the fourth active turn.
/// Negated by Reraise or instant death protection. If undead, Doom is lifted at counter zero.
/// </summary>
public class DoomStatus : StatusEffect
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
            // Check for Reraise
            var reraise = GetComponent<ReraiseStatus>();
            if (reraise != null)
            {
                // Reraise and Doom nullify each other
                var raiseCond = reraise.GetComponent<StatusCondition>();
                if (raiseCond != null)
                    raiseCond.Remove();

                var doomCond = GetComponent<StatusCondition>();
                if (doomCond != null)
                    doomCond.Remove();

                return;
            }

            // Check for undead status
            var undead = GetComponent<UndeadStatus>();
            if (undead != null)
            {
                // For undead, Doom is lifted instead of KO
                var cond = GetComponent<StatusCondition>();
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
            var doomCondition = GetComponent<StatusCondition>();
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
