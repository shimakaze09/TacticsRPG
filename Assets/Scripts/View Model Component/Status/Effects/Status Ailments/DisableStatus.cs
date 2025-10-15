using UnityEngine;

/// <summary>
/// Disable: Unit unable to act, evade, or use reaction abilities.
/// At end of AT, CT is decremented as if they had acted.
/// Lasts for 24 ticks (~2-3 turns).
/// </summary>
public class DisableStatus : TurnBasedStatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        // In FFT, Disable lasts 24 clock ticks
        remainingTurns = 3;

        if (owner)
        {
            // Prevent all actions
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        }

        if (stats != null)
        {
            // Disable evasion
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
        if (owner == unit && e.Exception.defaultToggle)
            e.Exception.FlipToggle();
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Disable evasion
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }
    }
}
