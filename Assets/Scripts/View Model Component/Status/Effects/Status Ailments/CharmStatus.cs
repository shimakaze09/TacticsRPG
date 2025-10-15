using UnityEngine;

/// <summary>
/// Charm: Unit perceives enemies as allies and allies as enemies.
/// Put under AI control. Lasts for 32 ticks (~3-4 turns).
/// Can only affect units of opposite gender (or any gender can affect monsters).
/// </summary>
public class CharmStatus : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    private void OnDisable()
    {
        // Restore original alliance if needed
    }

    // Method to check if target can be charmed based on gender
    public static bool CanCharm(Unit caster, Unit target)
    {
        // In FFT, charm can only affect units of opposite gender
        // or any gender can affect monsters
        // This is a simplified implementation
        return true;
    }
}
