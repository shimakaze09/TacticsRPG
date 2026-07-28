using System;
using UnityEngine;

public class InflictAbilityEffect : BaseAbilityEffect
{
    public int duration;
    public string statusName;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        var statusType = ResolveStatusType(statusName);
        if (statusType == null)
        {
            Debug.LogError($"Invalid Status Type: {statusName}");
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

    // Status data uses bare names ("Poison") while the classes are suffixed
    // ("PoisonStatus"), so try both spellings.
    private static Type ResolveStatusType(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var type = Type.GetType(name);
        if (type == null || !type.IsSubclassOf(typeof(StatusEffect)))
            type = Type.GetType(name + "Status");

        if (type != null && type.IsSubclassOf(typeof(StatusEffect)))
            return type;
        return null;
    }
}