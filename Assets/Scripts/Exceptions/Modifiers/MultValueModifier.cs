using UnityEngine;
using System.Collections;

public class MultValueModifier : ValueModifier
{
    public readonly float toMultiply;

    public MultValueModifier(int sortOrder, float toMultiply) : base(sortOrder)
    {
        this.toMultiply = toMultiply;
    }

    public override float Modify(float value)
    {
        return value * toMultiply;
    }
}