using System.Collections.Generic;
using UnityEngine;

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