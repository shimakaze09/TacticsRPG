using System;

/// <summary>
/// Applies a named status effect to the target for a duration (names
/// resolve through the typed StatusRegistry).
/// </summary>
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
        var unit = target.content.GetComponent<Unit>();
        StatusRegistry.Inflict(unit, statusName, duration);
        return 0;
    }

    // Kept as the AI's lookup point for whether a target already carries a
    // status before re-applying it; backed by the registry.
    public static Type ResolveStatusType(string name)
    {
        return StatusRegistry.Resolve(name);
    }
}