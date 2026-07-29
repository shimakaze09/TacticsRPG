using UnityEngine;

/// <summary>
/// Keeps the pending value at or above a floor (despite the name — see audit
/// note).
/// </summary>
public class MaxValueModifier : ValueModifier
{
    public float max;

    public MaxValueModifier(int sortOrder, float max) : base(sortOrder)
    {
        this.max = max;
    }

    public override float Modify(float fromValue, float toValue)
    {
        return Mathf.Max(toValue, max);
    }
}