using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMenuEntry : MonoBehaviour
{
    #region Public

    public void Reset()
    {
        State = States.None;
    }

    #endregion

    #region Enums

    [Flags]
    private enum States
    {
        None = 0,
        Selected = 1 << 0,
        Locked = 1 << 1
    }

    #endregion

    #region Properties

    public string Title
    {
        get => label.text;
        set => label.text = value;
    }

    public bool IsLocked
    {
        get => (State & States.Locked) != States.None;
        set
        {
            if (value)
                State |= States.Locked;
            else
                State &= ~States.Locked;
        }
    }

    public bool IsSelected
    {
        get => (State & States.Selected) != States.None;
        set
        {
            if (value)
                State |= States.Selected;
            else
                State &= ~States.Selected;
        }
    }

    private States State
    {
        get => state;
        set
        {
            if (state == value)
                return;
            state = value;

            if (IsLocked)
            {
                bullet.sprite = disabledSprite;
                label.color = Color.gray;
                label.outlineColor = new Color32(20, 36, 44, 255);
            }
            else if (IsSelected)
            {
                bullet.sprite = selectedSprite;
                label.color = new Color32(249, 210, 118, 255);
                label.outlineColor = new Color32(255, 160, 72, 255);
            }
            else
            {
                bullet.sprite = normalSprite;
                label.color = Color.white;
                label.outlineColor = new Color32(20, 36, 44, 255);
            }
        }
    }

    private States state;

    [SerializeField] private Image bullet;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField] private TextMeshProUGUI label;

    #endregion
}