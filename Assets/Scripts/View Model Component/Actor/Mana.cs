using UnityEngine;
using System.Collections;

public class Mana : MonoBehaviour
{
    #region Fields

    public int MP
    {
        get => stats[StatTypes.MP];
        set => stats[StatTypes.MP] = value;
    }

    public int MMP
    {
        get => stats[StatTypes.MMP];
        set => stats[StatTypes.MMP] = value;
    }

    private Unit unit;
    private Stats stats;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        stats = GetComponent<Stats>();
        unit = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        this.AddObserver(OnMPWillChange, Stats.WillChangeNotification(StatTypes.MP), stats);
        this.AddObserver(OnMMPDidChange, Stats.DidChangeNotification(StatTypes.MMP), stats);
        this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, unit);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnMPWillChange, Stats.WillChangeNotification(StatTypes.MP), stats);
        this.RemoveObserver(OnMMPDidChange, Stats.DidChangeNotification(StatTypes.MMP), stats);
        this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, unit);
    }

    #endregion

    #region Event Handlers

    private void OnMPWillChange(object sender, object args)
    {
        var vce = args as ValueChangeException;
        vce.AddModifier(new ClampValueModifier(int.MaxValue, 0, stats[StatTypes.MHP]));
    }

    private void OnMMPDidChange(object sender, object args)
    {
        var oldMMP = (int)args;
        if (MMP > oldMMP)
            MP += MMP - oldMMP;
        else
            MP = Mathf.Clamp(MP, 0, MMP);
    }

    private void OnTurnBegan(object sender, object args)
    {
        if (MP < MMP)
            MP += Mathf.Max(Mathf.FloorToInt(MMP * 0.1f), 1);
    }

    #endregion
}