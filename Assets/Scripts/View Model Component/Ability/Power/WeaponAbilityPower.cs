using System.Linq;

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
        var item = eq.GetItem(EquipSlots.Primary);
        var features = item.GetComponentsInChildren<StatModifierFeature>();

        return features.Where(t => t.type == StatTypes.ATK).Sum(t => t.amount);
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