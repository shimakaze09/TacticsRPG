using UnityEngine;

/// <summary>
/// Poison: At the end of each turn, take damage equal to 1/8 of maximum HP.
/// Lasts for 36 ticks (~3-4 turns). Opposed to Regen.
/// </summary>
public class PoisonStatus : StatusEffect
{
    [Tooltip("Damage as fraction of max HP (0.125 = 1/8 = 12.5%)")]
    public float damagePercent = 0.125f;

    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner != null)
            this.SubscribeToSender<TurnCompletedEvent>(OnTurnCompleted, owner);
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnCompletedEvent>(OnTurnCompleted, owner);
    }

    private void OnTurnCompleted(TurnCompletedEvent e)
    {
        if (stats == null)
            stats = GetComponentInParent<Stats>();

        if (stats == null)
            return;

        // Deal damage at end of turn
        int maxHP = stats[StatTypes.MHP];
        int currentHP = stats[StatTypes.HP];
        int damageAmount = Mathf.FloorToInt(maxHP * damagePercent);
        int newHP = Mathf.Max(currentHP - damageAmount, 0);

        stats.SetValue(StatTypes.HP, newHP, false);
    }
}
