using UnityEngine;

/// <summary>
/// Critical: Unit's HP is below 20% of maximum.
/// Doesn't do anything by itself; serves as visual indicator and activates "Critical:" reaction abilities.
/// </summary>
public class CriticalStatus : StatusEffect
{
    [Tooltip("HP percentage threshold for critical status (0.2 = 20%)")]
    public float criticalThreshold = 0.2f;

    private Stats stats;

    private void OnEnable()
    {
        stats = GetComponentInParent<Stats>();

        if (stats != null)
        {
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
            
            // Check immediately if we should remove
            CheckCriticalStatus();
        }
    }

    private void OnDisable()
    {
        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP && e.StatType != StatTypes.MHP)
            return;

        CheckCriticalStatus();
    }

    private void CheckCriticalStatus()
    {
        if (stats == null)
            return;

        int currentHP = stats[StatTypes.HP];
        int maxHP = stats[StatTypes.MHP];
        float hpPercent = (float)currentHP / maxHP;

        // If HP is above threshold, remove critical status
        if (hpPercent > criticalThreshold)
        {
            var cond = GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }

    public bool IsCritical()
    {
        return true;
    }
}
