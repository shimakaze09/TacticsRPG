using UnityEngine;

/// <summary>
/// Confuse: Unit takes random actions. Evasion of targets attacked by confused unit are doubled.
/// Prevents reaction abilities and special movement abilities.
/// Cancelled when afflicted unit takes damage.
/// </summary>
public class ConfuseStatus : DamageRemovableStatusEffect
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

    // Called by AI system to determine if unit should act randomly
    public bool IsConfused()
    {
        return true;
    }

    // Combat system should check this when calculating hit chance
    public bool DoublesTargetEvasion()
    {
        return true;
    }
}
