using UnityEngine;

/// <summary>
/// Petrify (Stone): Immobilizes target and flags them as defeated.
/// CT does not increment, cannot take actions or damage.
/// If all characters are petrified, game over.
/// </summary>
public class PetrifyStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner)
        {
            // Prevent all actions
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
            this.Subscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);
        }

        if (stats != null)
        {
            // Prevent CT gain and make invulnerable
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, stats);
        }
    }

    private void OnDisable()
    {
        this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        this.Unsubscribe<MovementCanMoveCheckEvent>(OnCanMoveCheck);

        if (stats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, stats);
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (owner == unit && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }

    private void OnCanMoveCheck(MovementCanMoveCheckEvent e)
    {
        var unit = e.Movement.GetComponentInParent<Unit>();
        if (owner == unit && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Prevent CT gain
        if (e.StatType == StatTypes.CTR)
        {
            var modifier = new MultDeltaModifier(0, 0);
            e.Exception.AddModifier(modifier);
        }

        // Make invulnerable to damage - keep HP constant
        if (e.StatType == StatTypes.HP)
        {
            // Set minimum and maximum to current HP to prevent any change
            var currentHP = stats[StatTypes.HP];
            var modifier = new ClampValueModifier(currentHP, currentHP, currentHP);
            e.Exception.AddModifier(modifier);
        }
    }
}
