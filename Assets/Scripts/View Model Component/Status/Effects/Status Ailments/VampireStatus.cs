using UnityEngine;

/// <summary>
/// Vampire: Unit can only use Vampire attack and automatically targets enemies.
/// All evade percentages drop to 0. Cannot use reaction/certain movement abilities.
/// If all party members are Vampire, game over.
/// </summary>
public class VampireStatus : StatusEffect
{
    private Unit owner;
    private Stats stats;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();

        if (owner)
        {
            // Force only Vampire attack
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        }

        if (stats != null)
        {
            // Set evasion to 0
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

        // Only allow Vampire attack
        // This needs integration with your ability system
        // if (!IsVampireAttack(e.Ability) && e.Exception.defaultToggle)
        //     e.Exception.FlipToggle();
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        // Set all evasion to 0
        if (e.StatType == StatTypes.EVD)
        {
            var modifier = new MultValueModifier(0, 0f);
            e.Exception.AddModifier(modifier);
        }
    }
}
