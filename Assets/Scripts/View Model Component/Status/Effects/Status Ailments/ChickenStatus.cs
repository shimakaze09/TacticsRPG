using UnityEngine;

/// <summary>
/// Chicken: Occurs when Bravery is below 10. Unit turns into chicken and runs away from all units.
/// Each turn restores Bravery by 1. Removed when Bravery > 10.
/// Attackers receive 1.5x Physical Attack bonus.
/// </summary>
public class ChickenStatus : StatusEffect
{
    [Tooltip("Minimum bravery to remove chicken status")]
    public int braveryThreshold = 10;

    [Tooltip("Bravery restored per turn")]
    public int braveryRestorePerTurn = 1;

    [Tooltip("Current bravery value (simulated if no BRV stat exists)")]
    public int currentBravery = 5;

    private Unit owner;
    private Stats stats;

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
        if (stats == null)
            stats = GetComponentInParent<Stats>();

        // Restore bravery each turn
        currentBravery = Mathf.Min(currentBravery + braveryRestorePerTurn, 100);

        // Check if bravery is above threshold
        if (currentBravery >= braveryThreshold)
        {
            var cond = GetComponent<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }

    public int GetBravery()
    {
        return currentBravery;
    }
}
