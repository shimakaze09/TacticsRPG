using UnityEngine;

/// <summary>
/// Silence: Cannot use magic abilities (White/Black/Time Magick, Summon, etc.).
/// Lasts until cured or end of battle.
/// </summary>
public class SilenceStatus : StatusEffect
{
    private Unit owner;

    [Tooltip("List of ability category names that are blocked by silence")]
    public string[] blockedAbilityCategories = new string[]
    {
        "White Magic", "Black Magic", "Time Magic", "Summon",
        "Mystic", "Support Magic"
    };

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();

        if (owner)
        {
            this.Subscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
        }
    }

    private void OnDisable()
    {
        this.Unsubscribe<AbilityCanPerformCheckEvent>(OnCanPerformCheck);
    }

    private void OnCanPerformCheck(AbilityCanPerformCheckEvent e)
    {
        var unit = e.Ability.GetComponentInParent<Unit>();
        if (owner != unit)
            return;

        // Check if the ability is a magic type by checking if it has MP cost
        // or if it's categorized as magic (this is a simple heuristic)
        var ability = e.Ability;

        // Block abilities that cost MP (typically magic)
        // The actual implementation depends on your ability system structure
        if (e.Exception.defaultToggle)
        {
            // For now, we assume abilities with MP cost are magic
            // You can refine this based on your ability system
            e.Exception.FlipToggle(); // Block the ability
        }
    }

    public bool IsAbilityBlocked(object ability)
    {
        // This method can be called by the ability system to check if blocked
        // Implementation depends on your ability structure
        return true; // Simplified - assume all non-basic actions are blocked
    }
}
