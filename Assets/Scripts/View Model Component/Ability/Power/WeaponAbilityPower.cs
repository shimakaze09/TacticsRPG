using System.Linq;

/// <summary>
/// Power source for basic weapon attacks: equipped weapon ATK bonuses, or the
/// job's base ATK when unarmed.
/// </summary>
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
        var item = eq != null ? eq.GetItem(EquipSlots.Primary) : null;
        if (item == null)
            return 0;

        var features = item.GetComponentsInChildren<StatModifierFeature>();
        return features.Where(t => t.type == StatTypes.ATK).Sum(t => t.amount);
    }

    private int UnarmedPower()
    {
        var jobManager = GetComponentInParent<JobManager>();
        var job = jobManager != null ? jobManager.CurrentJob : null;
        if (job == null)
            return 0;

        for (var i = 0; i < JobManager.statOrder.Length; i++)
            if (JobManager.statOrder[i] == StatTypes.ATK)
                return job.baseStats[i];
        return 0;
    }
}