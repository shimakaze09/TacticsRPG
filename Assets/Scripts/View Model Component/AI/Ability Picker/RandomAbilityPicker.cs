using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ability picker that chooses randomly from a list of pickers.
/// </summary>
public class RandomAbilityPicker : BaseAbilityPicker
{
    public List<BaseAbilityPicker> pickers;

    public override void Pick(PlanOfAttack plan)
    {
        var index = Random.Range(0, pickers.Count);
        var p = pickers[index];
        p.Pick(plan);
    }
}