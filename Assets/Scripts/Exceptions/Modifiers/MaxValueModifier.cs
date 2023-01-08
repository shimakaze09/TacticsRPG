using UnityEngine;
using System.Collections;

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