using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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