using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponAbilityPower : BaseAbilityPower
{
    protected override int GetBaseAttack()
    {
        return GetComponentInParent<Stats>()[StatTypes.ATK];
    }

    protected override int GetBaseDefense(Unit target)
    {
        return target.GetComponent<Stats>()[StatTypes.DEF];
    }

    protected override int GetPower()
    {
        var power = PowerFromEquippedWeapon();
        return power > 0 ? power : UnarmedPower();
    }

    private int PowerFromEquippedWeapon()
    {
        var eq = GetComponentInParent<Equipment>();
        Equippable item = eq.GetItem(EquipSlots.Primary);
        var features = item.GetComponentsInChildren<StatModifierFeature>();

        var power = features.Where(t => t.type == StatTypes.ATK).Sum(t => t.amount);

        return power;
    }

    private int UnarmedPower()
    {
        var job = GetComponentInParent<Job>();
        for (var i = 0; i < Job.statOrder.Length; i++)
            if (Job.statOrder[i] == StatTypes.ATK)
                return job.baseStats[i];

        return 0;
    }
}