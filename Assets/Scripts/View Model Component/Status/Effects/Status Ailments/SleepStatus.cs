using UnityEngine;

/// <summary>
/// Sleep: Unit will not gain CT, cannot evade, cannot use reaction abilities.
/// Attackers get +50% physical attack bonus. Wakes up when taking HP damage.
/// Lasts for 60 ticks (~6 turns) if not damaged.
/// </summary>
public class SleepStatus : DamageRemovableStatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner)
        {
            // Prevent actions
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        }

        if (stats != null)
        {
            // Prevent evasion and disable CT gain
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
        if (owner == unit)
        {
            if (e.Exception.defaultToggle)
                e.Exception.FlipToggle(); // Prevent performing ability while asleep
        }
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Disable evasion
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }

        // Prevent CT gain
        if (e.StatType == StatTypes.CTR)
        {
            var modifier = new MultDeltaModifier(0, 0);
            e.Exception.AddModifier(modifier);
        }
    }
}
