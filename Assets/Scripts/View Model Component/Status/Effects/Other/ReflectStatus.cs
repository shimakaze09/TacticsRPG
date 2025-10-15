using UnityEngine;

/// <summary>
/// Reflect: Reflects magic spells marked with reflect symbol (☇).
/// Target tile changes from target to a tile in same position as if target were casting the spell.
/// Lasts for 32 clock ticks (~3-4 turns).
/// </summary>
public class ReflectStatus : TurnBasedStatusEffect
{
    private Unit owner;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();

        // In FFT, Reflect lasts 32 clock ticks
        remainingTurns = 4;
    }

    private void OnDisable()
    {
        // Cleanup
    }

    // Called by magic system to check if spell should be reflected
    public bool ShouldReflectSpell(object spell)
    {
        // In FFT, most offensive magic is reflectable
        // Magic marked with the reflect symbol (☇) is reflected
        // Defensive magic (buffs on self) typically cannot be reflected
        return true;
    }

    // Calculate reflection target position
    public Vector3 GetReflectionTarget(Vector3 casterPos, Vector3 originalTargetPos)
    {
        // Mirror the target position as if the target were casting back
        Vector3 direction = originalTargetPos - casterPos;
        return originalTargetPos + direction;
    }

    public bool HasReflect()
    {
        return true;
    }
}
