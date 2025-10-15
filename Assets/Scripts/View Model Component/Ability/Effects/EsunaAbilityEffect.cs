using System;
using System.Collections.Generic;

public class EsunaAbilityEffect : BaseAbilityEffect
{
    private static HashSet<Type> _curableTypes;

    private static HashSet<Type> CurableTypes
    {
        get
        {
            if (_curableTypes == null)
                _curableTypes = new HashSet<Type>
                {
                    // Status Ailments (curable)
                    typeof(PoisonStatus),
                    typeof(BlindStatus),
                    typeof(SilenceStatus),
                    typeof(SlowStatus),
                    typeof(ImmobilizeStatus),
                    typeof(DisableStatus),
                    typeof(SleepStatus),
                    typeof(ConfuseStatus),
                    // Note: Some status effects like Petrify, KO, Undead are typically NOT curable by Esuna
                    // Add or remove types based on your game design
                };

            return _curableTypes;
        }
    }

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        var defender = target.content.GetComponent<Unit>();
        var status = defender.GetComponentInChildren<Status>();

        if (status == null)
            return 0;

        var candidates = status.GetComponentsInChildren<DurationStatusCondition>();
        foreach (var condition in candidates)
        {
            var effect = condition.GetComponentInParent<StatusEffect>();
            if (effect != null && CurableTypes.Contains(effect.GetType()))
                condition.Remove();
        }

        return 0;
    }
}
