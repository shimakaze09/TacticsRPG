﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseAbilityEffect : MonoBehaviour
{
    #region Consts & Notifications

    protected const int minDamage = -999;
    protected const int maxDamage = 999;

    public const string GetAttackNotification = "BaseAbilityEffect.GetAttackNotification";
    public const string GetDefenseNotification = "BaseAbilityEffect.GetDefenseNotification";
    public const string GetPowerNotification = "BaseAbilityEffect.GetPowerNotification";
    public const string TweakDamageNotification = "BaseAbilityEffect.TweakDamageNotification";

    public const string MissedNotification = "BaseAbilityEffect.MissedNotification";
    public const string HitNotification = "BaseAbilityEffect.HitNotification";

    #endregion

    #region Public

    public abstract int Predict(Tile target);

    public void Apply(Tile target)
    {
        if (GetComponent<AbilityEffectTarget>().IsTarget(target) == false)
            return;

        if (GetComponent<HitRate>().RollForHit(target))
            this.PostNotification(HitNotification, OnApply(target));
        else
            this.PostNotification(MissedNotification);
    }

    #endregion

    #region Protected

    protected abstract int OnApply(Tile target);

    protected virtual int GetStat(Unit attacker, Unit target, string notification, int startValue)
    {
        var mods = new List<ValueModifier>();
        var info = new Info<Unit, Unit, List<ValueModifier>>(attacker, target, mods);
        this.PostNotification(notification, info);
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