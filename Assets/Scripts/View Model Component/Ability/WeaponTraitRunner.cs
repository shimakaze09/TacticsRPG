using UnityEngine;

/// <summary>
/// Runs the equipped weapon's conditional and on-hit traits for the basic
/// attack: FlankBonus tweaks damage from behind, and after a connect Recoil
/// feeds damage back, StatusOnHit rolls its infliction, Lifesteal heals the
/// attacker, WindedAfterStrike Throttles them. Lives on the Attack ability;
/// new trait types plug in here.
/// </summary>
public class WeaponTraitRunner : MonoBehaviour
{
    private void OnEnable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
        {
            this.SubscribeToSender<AbilityHitEvent>(OnHit, effect);
            this.SubscribeToSender<TweakDamageEvent>(OnTweakDamage, effect);
        }
    }

    private void OnDisable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
        {
            this.UnsubscribeFromSender<AbilityHitEvent>(OnHit, effect);
            this.UnsubscribeFromSender<TweakDamageEvent>(OnTweakDamage, effect);
        }
    }

    // Conditional damage tweaks resolved during the damage calculation
    private void OnTweakDamage(TweakDamageEvent e)
    {
        var attacker = GetComponentInParent<Unit>();
        if (attacker == null || e.Attacker != attacker || e.Target == null)
            return;

        var gear = GearCatalog.EquippedWeapon(this);
        if (gear == null || gear.traits == null)
            return;

        foreach (var trait in gear.traits)
        {
            if (trait.type == GearTraitType.FlankBonus &&
                attacker.GetFacing(e.Target) == Facings.Back)
                e.Modifiers.Add(new MultValueModifier(90, 1f + trait.value / 100f));
        }
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

                case GearTraitType.Lifesteal:
                    // The wound feeds the wielder
                    if (e.Damage < 0)
                    {
                        var stats = attacker.GetComponent<Stats>();
                        stats[StatTypes.HP] -= Mathf.FloorToInt(e.Damage * trait.value / 100f);
                    }

                    break;

                case GearTraitType.StatusOnHit:
                    RollStatusOnHit(e.Target, trait);
                    break;
            }
        }
    }

    // Chance roll, then the same reflection-infliction the Inflict effect uses
    private static void RollStatusOnHit(Unit target, GearTraitData trait)
    {
        if (target == null || Random.Range(0, 100) >= trait.value)
            return;

        var statusType = InflictAbilityEffect.ResolveStatusType(trait.tag);
        if (statusType == null)
        {
            Debug.LogError($"[WeaponTraitRunner] Unknown status '{trait.tag}'");
            return;
        }

        // Don't stack a second copy of something the target already carries
        if (target.GetComponentInChildren(statusType) != null)
            return;

        var add = typeof(Status).GetMethod("Add");
        var constructed = add.MakeGenericMethod(statusType, typeof(DurationStatusCondition));
        var condition = constructed.Invoke(target.GetComponent<Status>(), null) as DurationStatusCondition;
        condition.duration = trait.duration;
    }
}
