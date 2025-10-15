using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseAbilityEffect : MonoBehaviour
{
    #region Consts

    protected const int minDamage = -999;
    protected const int maxDamage = 999;

    #endregion

    #region Public

    public abstract int Predict(Tile target);

    public void Apply(Tile target)
    {
        if (GetComponent<AbilityEffectTarget>().IsTarget(target) == false)
            return;

        var attacker = GetComponentInParent<Unit>();
        var targetUnit = target.content?.GetComponent<Unit>();

        if (GetComponent<HitRate>().RollForHit(target))
        {
            var damage = OnApply(target);
            this.Publish(new AbilityHitEvent(attacker, targetUnit, damage));
        }
        else
        {
            this.Publish(new AbilityMissedEvent(attacker, targetUnit));
        }
    }

    #endregion

    #region Protected

    protected abstract int OnApply(Tile target);

    protected virtual int GetStat(Unit attacker, Unit target, System.Type eventType, int startValue)
    {
        var mods = new List<ValueModifier>();

        // Publish the appropriate event based on type
        if (eventType == typeof(GetAttackStatEvent))
            this.Publish(new GetAttackStatEvent(attacker, target, mods));
        else if (eventType == typeof(GetDefenseStatEvent))
            this.Publish(new GetDefenseStatEvent(attacker, target, mods));
        else if (eventType == typeof(GetPowerEvent))
            this.Publish(new GetPowerEvent(attacker, target, mods));
        else if (eventType == typeof(TweakDamageEvent))
            this.Publish(new TweakDamageEvent(attacker, target, mods));

        mods.Sort(Compare);

        var value = mods.Aggregate<ValueModifier, float>(startValue, (current, t) => t.Modify(startValue, current));

        var retValue = Mathf.FloorToInt(value);
        retValue = Mathf.Clamp(retValue, minDamage, maxDamage);
        return retValue;
    }

    private int Compare(ValueModifier x, ValueModifier y)
    {
        return x.sortOrder.CompareTo(y.sortOrder);
    }

    #endregion
}