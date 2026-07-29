using System;
using System.Collections.Generic;

/// <summary>
/// Removes curable negative statuses from the target (Sawbones' Antitoxin).
/// </summary>
public class CleanseAbilityEffect : BaseAbilityEffect
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
                    typeof(SepsisStatus),
                    typeof(StaticStatus),
                    typeof(DeadAirStatus),
                    typeof(ThrottleStatus),
                    typeof(PinnedStatus),
                    typeof(JammedStatus),
                    typeof(BlackoutStatus),
                    typeof(ScrambledStatus),
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
