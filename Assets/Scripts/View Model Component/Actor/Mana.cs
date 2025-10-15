using UnityEngine;

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
        this.SubscribeToSender<StatWillChangeEvent>(OnMPWillChange, stats);
        this.SubscribeToSender<StatDidChangeEvent>(OnMMPDidChange, stats);
        this.SubscribeToSender<TurnBeganEvent>(OnTurnBegan, unit);
    }

    private void OnDisable()
    {
        this.UnsubscribeFromSender<StatWillChangeEvent>(OnMPWillChange, stats);
        this.UnsubscribeFromSender<StatDidChangeEvent>(OnMMPDidChange, stats);
        this.UnsubscribeFromSender<TurnBeganEvent>(OnTurnBegan, unit);
    }

    #endregion

    #region Event Handlers

    private void OnMPWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.MP)
            return;

        e.Exception.AddModifier(new ClampValueModifier(int.MaxValue, 0, stats[StatTypes.MHP]));
    }

    private void OnMMPDidChange(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.MMP)
            return;

        var oldMMP = e.OldValue;
        if (MMP > oldMMP)
            MP += MMP - oldMMP;
        else
            MP = Mathf.Clamp(MP, 0, MMP);
    }

    private void OnTurnBegan(TurnBeganEvent e)
    {
        if (MP < MMP)
            MP += Mathf.Max(Mathf.FloorToInt(MMP * 0.1f), 1);
    }

    #endregion
}