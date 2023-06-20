using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stats : MonoBehaviour
{
    #region Notifications

    public static string WillChangeNotification(StatTypes type)
    {
        if (!_willChangeNotifications.ContainsKey(type))
            _willChangeNotifications.Add(type, $"Stats.{type}WillChange");
        return _willChangeNotifications[type];
    }

    public static string DidChangeNotification(StatTypes type)
    {
        if (!_didChangeNotifications.ContainsKey(type))
            _didChangeNotifications.Add(type, $"Stats.{type}DidChange");
        return _didChangeNotifications[type];
    }

    private static Dictionary<StatTypes, string> _willChangeNotifications = new();
    private static Dictionary<StatTypes, string> _didChangeNotifications = new();

    #endregion

    #region Fields / Properties

    public int this[StatTypes s]
    {
        get => _data[(int)s];
        set => SetValue(s, value, true);
    }

    private int[] _data = new int[(int)StatTypes.Count];

    #endregion

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

            // The notification is unique per stat type
            this.PostNotification(WillChangeNotification(type), exc);

            // Did anything modify the value?
            value = Mathf.FloorToInt(exc.GetModifiedValue());

            // Did something nullify the change?
            if (exc.toggle == false || value == oldValue)
                return;
        }

        _data[(int)type] = value;
        this.PostNotification(DidChangeNotification(type), oldValue);
    }

    #endregion
}