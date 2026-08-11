using UnityEngine;

/// <summary>
/// Resolves a unit's basic strike — the "Attack" ability every combatant
/// receives from UnitFactory (the Common/Attack prefab). The AI's fallback
/// paths must aim for this specific ability rather than whatever descendant
/// happens to be found first (a unit's job catalog is parented before its
/// attack, so "first Ability" is usually a job skill, not the basic attack).
/// Degrades loudly: warns and returns the first ability found when no
/// "Attack" exists, warns and returns null when the unit has none at all.
/// </summary>
public static class BasicAttackResolver
{
    /// <summary>The GameObject name UnitFactory gives every basic strike.</summary>
    public const string AbilityName = "Attack";

    /// <summary>
    /// The unit's basic attack; falls back to its first ability (with a
    /// diagnostic) when the basic attack is missing, or null when the unit
    /// carries no abilities at all.
    /// </summary>
    public static Ability Resolve(Unit unit)
    {
        if (unit == null)
            return null;

        Ability first = null;
        foreach (var ability in unit.GetComponentsInChildren<Ability>())
        {
            if (ability.name == AbilityName)
                return ability;
            if (first == null)
                first = ability;
        }

        if (first != null)
            Debug.LogWarning($"[AI] {unit.name} has no '{AbilityName}' ability — falling back to '{first.name}'");
        else
            Debug.LogWarning($"[AI] {unit.name} has no abilities at all — no attack plan possible");

        return first;
    }
}
