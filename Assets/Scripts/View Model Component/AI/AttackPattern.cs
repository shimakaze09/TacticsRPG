using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cycles through a list of ability pickers turn by turn — the Easy AI's
/// scripted rotation.
/// </summary>
public class AttackPattern : MonoBehaviour
{
    private int index;
    public List<BaseAbilityPicker> pickers;

    public void Pick(PlanOfAttack plan)
    {
        pickers[index].Pick(plan);
        index++;
        if (index >= pickers.Count)
            index = 0;
    }
}