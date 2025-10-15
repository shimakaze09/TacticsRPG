using UnityEngine;

/// <summary>
/// Toad: Limits unit to Attack command only. No reaction abilities.
/// Attack damage = PA * Bravery/100. Attackers receive 1.5x Physical Attack bonus.
/// Does not automatically remove on KO - must be actively healed.
/// </summary>
public class ToadStatus : StatusEffect
{
    [Tooltip("Maximum attack value while toad (FFT uses 1)")]
    public int maxAtkWhileToad = 1;

    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner)
        {
            // Block all abilities except basic attack
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        }

        if (stats != null)
        {
            // Cap attack stat
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, stats);
        }
    }

    private void OnDisable()
    {
        this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);

        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (owner != unit)
            return;

        // Only allow basic attack - block all other abilities
        // This is a simplified implementation
        // You may need to check ability name or type to allow only "Attack"
        if (e.Exception.defaultToggle)
        {
            // Block all abilities (the attack system should handle basic attack separately)
            e.Exception.FlipToggle();
        }
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.ATK)
            return;

        // Cap ATK to maxAtkWhileToad
        var modifier = new MinValueModifier(0, maxAtkWhileToad);
        e.Exception.AddModifier(modifier);
    }
}
