using UnityEngine;

/// <summary>
/// Stop: Unit stops moving and CT meter is frozen.
/// Cannot evade or use reaction abilities.
/// Wears off after 20 ticks (~2 turns).
/// </summary>
public class StopStatus : StatusEffect
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
            // Freeze CT and disable evasion
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
        // Freeze CT
        if (e.StatType == StatTypes.CTR)
        {
            var modifier = new MultDeltaModifier(0, 0);
            e.Exception.AddModifier(modifier);
        }

        // Disable evasion
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }
    }
}
