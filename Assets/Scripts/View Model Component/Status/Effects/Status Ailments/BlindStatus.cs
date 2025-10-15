using UnityEngine;

/// <summary>
/// Blindness: If a blind unit uses a physical attack, all target evade rates are doubled.
/// Lasts until end of battle or until cured.
/// </summary>
public class BlindStatus : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    private void OnDisable()
    {
        // Cleanup if needed
    }

    // Method to be called by combat system
    public bool IsBlind()
    {
        return true;
    }
}
