using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    #region Public

    public void SetValue(StatTypes type, int value, bool allowExceptions)
    {
        var oldValue = this[type];
        if (oldValue == value)
            return;

        if (allowExceptions)
        {
            // Allow exceptions to the rule here
            var exc = new ValueChangeException(oldValue, value);

            // Publish the WillChange event for this stat type
            this.Publish(new StatWillChangeEvent(this, type, exc));

            // Did anything modify the value?
            value = Mathf.FloorToInt(exc.GetModifiedValue());

            // Did something nullify the change?
            if (exc.toggle == false || value == oldValue)
                return;
        }

        _data[(int)type] = value;

        // Publish the DidChange event for this stat type
        this.Publish(new StatDidChangeEvent(this, type, oldValue, value));
    }

    #endregion



    #region Fields / Properties

    public int this[StatTypes s]
    {
        get => _data[(int)s];
        set => SetValue(s, value, true);
    }

    private readonly int[] _data = new int[(int)StatTypes.Count];

    #endregion
}