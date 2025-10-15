using UnityEngine;

/// <summary>
/// Float: Units are positioned 1h above ground, immune to earth-elemental attacks.
/// Unaffected by move penalties. Does not wear off naturally.
/// </summary>
public class FloatStatus : StatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    private void OnDisable()
    {
        // Restore normal height and remove immunities
    }

    // Example method that could be called by damage calculation system
    public bool IsImmuneToElement(string element)
    {
        return element == "Earth" || element == "earth";
    }
}
