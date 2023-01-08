using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ValueModifier : Modifier
{
    public ValueModifier(int sortOrder) : base(sortOrder)
    {
    }

    public abstract float Modify(float fromValue, float toValue);
}