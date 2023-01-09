using System;
using System.Collections.Generic;

public class EsunaAbilityEffect : BaseAbilityEffect
{
    private static HashSet<Type> _curableTypes;

    private static HashSet<Type> CurableTypes =>
        _curableTypes ??= new HashSet<Type>
        {
            typeof(PoisonStatusEffect),
            typeof(BlindStatusEffect)
        };

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        var status = defender.GetComponentInChildren<Status>();

        var candidates = status.GetComponentsInChildren<DurationStatusCondition>();
        foreach (var candidate in candidates)
        {
            var effect = candidate.GetComponentInParent<StatusEffect>();
            if (CurableTypes.Contains(effect.GetType()))
                candidate.Remove();
        }

        return 0;
    }
}