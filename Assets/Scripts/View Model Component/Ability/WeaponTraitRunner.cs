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
    private readonly EventSubscriptions subscriptions = new();

    private void OnEnable()
    {
        var ability = GetComponentInParent<Ability>();
        if (ability == null)
            return;

        foreach (var effect in ability.GetComponentsInChildren<BaseAbilityEffect>())
        {
            subscriptions.SubscribeToSender<AbilityHitEvent>(OnHit, effect);
            subscriptions.SubscribeToSender<TweakDamageEvent>(OnTweakDamage, effect);
        }
    }

    private void OnDisable()
    {
        subscriptions.Clear();
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
            switch (trait.type)
            {
                case GearTraitType.FlankBonus when attacker.GetFacing(e.Target) == Facings.Back:
                    e.Modifiers.Add(new MultValueModifier(90, 1f + trait.value / 100f));
                    break;

                // Finisher: the target is already staggering (below 30% HP)
                case GearTraitType.Execute when TargetHpFraction(e.Target) < 0.3f:
                    e.Modifiers.Add(new MultValueModifier(90, 1f + trait.value / 100f));
                    break;

                // First blood: the target hasn't been touched yet
                case GearTraitType.Opener when TargetHpFraction(e.Target) >= 1f:
                    e.Modifiers.Add(new MultValueModifier(90, 1f + trait.value / 100f));
                    break;

                case GearTraitType.TerrainBonus when attacker.tile != null &&
                                                     attacker.tile.terrain.ToString() == trait.tag:
                    e.Modifiers.Add(new MultValueModifier(90, 1f + trait.value / 100f));
                    break;
            }
        }
    }

    private static float TargetHpFraction(Unit target)
    {
        var stats = target.GetComponent<Stats>();
        if (stats == null || stats[StatTypes.MHP] <= 0)
            return 1f;
        return (float)stats[StatTypes.HP] / stats[StatTypes.MHP];
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

                case GearTraitType.MpBurn:
                    // Burn the target's casting reserve alongside the wound
                    if (e.Damage < 0 && e.Target != null)
                    {
                        var targetStats = e.Target.GetComponent<Stats>();
                        if (targetStats != null)
                            targetStats[StatTypes.MP] -= Mathf.Min(trait.value, targetStats[StatTypes.MP]);
                    }

                    break;
            }
        }
    }

    // Chance roll, then a typed infliction through the registry
    private static void RollStatusOnHit(Unit target, GearTraitData trait)
    {
        if (target == null || Random.Range(0, 100) >= trait.value)
            return;

        // Don't stack a second copy of something the target already carries
        var statusType = StatusRegistry.Resolve(trait.tag);
        if (statusType != null && target.GetComponentInChildren(statusType) != null)
            return;

        StatusRegistry.Inflict(target, trait.tag, trait.duration);
    }
}
