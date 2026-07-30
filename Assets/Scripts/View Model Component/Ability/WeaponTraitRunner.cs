using UnityEngine;

/// <summary>
/// Runs the equipped weapon's on-hit traits when the basic attack connects:
/// Recoil feeds part of each hit back into the attacker, WindedAfterStrike
/// Throttles the attacker after the swing. Lives on the Attack ability;
/// new on-hit trait types plug in here.
/// </summary>
public class WeaponTraitRunner : MonoBehaviour
{
    private void OnEnable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
            this.SubscribeToSender<AbilityHitEvent>(OnHit, effect);
    }

    private void OnDisable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
            this.UnsubscribeFromSender<AbilityHitEvent>(OnHit, effect);
    }

    private void OnHit(AbilityHitEvent e)
    {
        var attacker = GetComponentInParent<Unit>();
        if (attacker == null || e.Attacker != attacker || e.Target == attacker)
            return;

        var gear = GearCatalog.EquippedWeapon(this);
        if (gear == null || gear.traits == null)
            return;

        foreach (var trait in gear.traits)
        {
            switch (trait.type)
            {
                case GearTraitType.Recoil:
                    // e.Damage is negative for real damage; heals feed nothing back
                    if (e.Damage < 0)
                    {
                        var stats = attacker.GetComponent<Stats>();
                        stats[StatTypes.HP] += Mathf.FloorToInt(e.Damage * trait.value / 100f);
                    }

                    break;

                case GearTraitType.WindedAfterStrike:
                    // Once per swing: a sweep hitting three shouldn't stack three
                    if (attacker.GetComponentInChildren<ThrottleStatus>() == null)
                    {
                        var status = attacker.GetComponent<Status>();
                        var condition = status.Add<ThrottleStatus, DurationStatusCondition>();
                        condition.duration = trait.value;
                    }

                    break;
            }
        }
    }
}
