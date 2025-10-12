using System;
using UnityEngine;

internal class Repeater
{
    private const float threshold = 0.5f;
    private const float rate = 0.25f;
    private readonly string _axis;
    private bool _hold;
    private float _next;

    public Repeater(string axisName)
    {
        _axis = axisName;
    }

    public int Update()
    {
        var retValue = 0;
        var value = Mathf.RoundToInt(Input.GetAxisRaw(_axis));

        if (value != 0)
        {
            if (Time.time > _next)
            {
                retValue = value;
                _next = Time.time + (_hold ? rate : threshold);
                _hold = true;
            }
        }
        else
        {
            _hold = false;
            _next = 0;
        }

        return retValue;
    }
}

public class InputController : MonoBehaviour
{
    private readonly string[] _buttons = { "Fire1", "Fire2", "Fire3" };

    private readonly Repeater _hor = new("Horizontal");
    private readonly Repeater _ver = new("Vertical");
    public static event EventHandler<InfoEventArgs<Point>> moveEvent;
    public static event EventHandler<InfoEventArgs<int>> fireEvent;

    private void Update()
    {
        var x = _hor.Update();
        var y = _ver.Update();
        if (x != 0 || y != 0) moveEvent?.Invoke(this, new InfoEventArgs<Point>(new Point(x, y)));

        for (var i = 0; i < 3; i++)
            if (Input.GetButtonUp(_buttons[i]))
                fireEvent?.Invoke(this, new InfoEventArgs<int>(i));
    }
}