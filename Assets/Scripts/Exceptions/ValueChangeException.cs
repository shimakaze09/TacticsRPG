using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ValueChangeException : BaseException
{
    #region Fields / Properties

    public readonly float fromValue, toValue;
    public float delta => toValue - fromValue;
    private List<ValueModifier> modifiers;

    #endregion

    #region Constructor

    public ValueChangeException(float fromValue, float toValue) : base(true)
    {
        this.fromValue = fromValue;
        this.toValue = toValue;
    }

    #endregion

    #region Public

    public void AddModifier(ValueModifier m)
    {
        modifiers ??= new List<ValueModifier>();
        modifiers.Add(m);
    }

    public float GetModifiedValue()
    {
        var value = toValue;
        if (modifiers == null) return value;

        modifiers.Sort(Compare);

        return modifiers.Aggregate(value, (current, modifier) => modifier.Modify(current));
    }

    #endregion

    #region Private

    private int Compare(ValueModifier x, ValueModifier y)
    {
        return x.sortOrder.CompareTo(y.sortOrder);
    }

    #endregion
}