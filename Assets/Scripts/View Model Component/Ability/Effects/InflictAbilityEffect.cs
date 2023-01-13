using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class InflictAbilityEffect : BaseAbilityEffect
{
    public string statusName;
    public int duration;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        var statusType = Type.GetType(statusName);
        if (statusType == null || !statusType.IsSubclassOf(typeof(StatusEffect)))
        {
            Debug.LogError("Invalid Status Type");
            return 0;
        }

        var mi = typeof(Status).GetMethod("Add");
        var types = new[] { statusType, typeof(DurationStatusCondition) };
        var constructed = mi.MakeGenericMethod(types);

        var status = target.content.GetComponent<Status>();
        var retValue = constructed.Invoke(status, null);

        var condition = retValue as DurationStatusCondition;
        condition.duration = duration;
        return 0;
    }
}