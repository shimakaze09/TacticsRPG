using UnityEngine;

/// <summary>
/// Regen: At the end of each turn, recover 1/8 of maximum HP.
/// Lasts for 32 clock ticks. Opposed to Poison.
/// </summary>
public class RegenStatus : StatusEffect
{
    [Tooltip("HP recovery as fraction of max HP (0.125 = 1/8 = 12.5%)")]
    public float healPercent = 0.125f;

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

        // Recover HP at end of turn
        int maxHP = stats[StatTypes.MHP];
        int currentHP = stats[StatTypes.HP];
        int healAmount = Mathf.FloorToInt(maxHP * healPercent);
        int newHP = Mathf.Min(currentHP + healAmount, maxHP);

        stats.SetValue(StatTypes.HP, newHP, false);
    }
}
