using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cycles through a list of ability pickers turn by turn — the Easy AI's
/// scripted rotation. An empty pattern or a missing picker entry is authoring
/// damage, not a crash: it warns and leaves the plan blank so the brain falls
/// back to the unit's basic attack.
/// </summary>
public class AttackPattern : MonoBehaviour
{
    private int index;
    public List<BaseAbilityPicker> pickers;

    /// <summary>
    /// Asks the current picker in the rotation to fill the plan, then
    /// advances the rotation. Leaves the plan untouched (with a diagnostic)
    /// when the pattern has no usable picker at this step.
    /// </summary>
    public void Pick(PlanOfAttack plan)
    {
        if (pickers == null || pickers.Count == 0)
        {
            Debug.LogWarning($"[AI] {name} has an empty attack pattern — falling back to the basic attack");
            return;
        }

        if (index >= pickers.Count)
            index = 0;

        var picker = pickers[index];
        index++;
        if (index >= pickers.Count)
            index = 0;

        if (picker == null)
        {
            Debug.LogWarning($"[AI] {name} attack pattern has a missing picker entry — falling back to the basic attack");
            return;
        }

        picker.Pick(plan);
    }
}
