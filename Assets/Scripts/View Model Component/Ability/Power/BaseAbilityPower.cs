using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Feeds the damage pipeline: contributes the attacker's stat, target's defense
/// stat, and the ability's power to the stat-query events.
/// </summary>
public abstract class BaseAbilityPower : MonoBehaviour
{
    protected abstract int GetBaseAttack();
    protected abstract int GetBaseDefense(Unit target);
    protected abstract int GetPower();

    private void OnEnable()
    {
        // Subscribe to events from any BaseAbilityEffect within our same Ability
        // This ensures we only respond to our own ability's calculations
        var myAbility = GetComponentInParent<Ability>();
        if (myAbility != null)
        {
            // Subscribe to events from all effects in this ability
            var effects = myAbility.GetComponentsInChildren<BaseAbilityEffect>();
            foreach (var effect in effects)
            {
                this.SubscribeToSender<GetAttackStatEvent>(OnGetBaseAttack, effect);
                this.SubscribeToSender<GetDefenseStatEvent>(OnGetBaseDefense, effect);
                this.SubscribeToSender<GetPowerEvent>(OnGetPower, effect);
            }
        }
    }

    private void OnDisable()
    {
        var myAbility = GetComponentInParent<Ability>();
        if (myAbility != null)
        {
            var effects = myAbility.GetComponentsInChildren<BaseAbilityEffect>();
            foreach (var effect in effects)
            {
                this.UnsubscribeFromSender<GetAttackStatEvent>(OnGetBaseAttack, effect);
                this.UnsubscribeFromSender<GetDefenseStatEvent>(OnGetBaseDefense, effect);
                this.UnsubscribeFromSender<GetPowerEvent>(OnGetPower, effect);
            }
        }
    }

    private void OnGetBaseAttack(GetAttackStatEvent e)
    {
        if (IsMyUnit(e.Attacker))
        {
            e.Modifiers.Add(new AddValueModifier(0, GetBaseAttack()));
        }
    }

    private void OnGetBaseDefense(GetDefenseStatEvent e)
    {
        // This power component belongs to the attacker's ability, so the
        // ownership check must be against the attacker; the defense value
        // itself is still read from the target.
        if (IsMyUnit(e.Attacker))
        {
            e.Modifiers.Add(new AddValueModifier(0, GetBaseDefense(e.Target)));
        }
    }

    private void OnGetPower(GetPowerEvent e)
    {
        if (IsMyUnit(e.Attacker))
        {
            e.Modifiers.Add(new AddValueModifier(0, GetPower()));
        }
    }

    private bool IsMyUnit(Unit unit)
    {
        // Check if the unit is the one that owns this component
        var myUnit = GetComponentInParent<Unit>();
        return myUnit == unit;
    }
}