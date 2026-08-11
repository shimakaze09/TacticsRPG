using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ability picker that chooses randomly from a list of pickers. An empty or
/// missing list warns and degrades to the owner's basic attack instead of
/// throwing — malformed authoring must never crash a turn.
/// </summary>
public class RandomAbilityPicker : BaseAbilityPicker
{
    public List<BaseAbilityPicker> pickers;

    /// <summary>Delegates to one randomly chosen sub-picker; basic attack when the list is unusable.</summary>
    public override void Pick(PlanOfAttack plan)
    {
        if (pickers == null || pickers.Count == 0)
        {
            Debug.LogWarning($"[AI] {name} has no pickers to choose from — falling back to the basic attack");
            PickDefault(plan);
            return;
        }

        var index = Random.Range(0, pickers.Count);
        var p = pickers[index];
        if (p == null)
        {
            Debug.LogWarning($"[AI] {name} rolled a missing picker entry — falling back to the basic attack");
            PickDefault(plan);
            return;
        }

        p.Pick(plan);
    }

    // Fallback plan: the owner's basic attack against foes
    private void PickDefault(PlanOfAttack plan)
    {
        plan.ability = Default();
        plan.target = Targets.Foe;
    }
}
