using UnityEngine;

public class Health : MonoBehaviour
{
    #region Fields & Properties

    public int HP
    {
        get => stats[StatTypes.HP];
        set => stats[StatTypes.HP] = value;
    }

    public int MHP
    {
        get => stats[StatTypes.MHP];
        set => stats[StatTypes.MHP] = value;
    }

    public int MinHP = 0;
    private Stats stats;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        stats = GetComponent<Stats>();
    }

    private void OnEnable()
    {
        this.SubscribeToSender<StatWillChangeEvent>(OnHPWillChange, stats);
        this.SubscribeToSender<StatDidChangeEvent>(OnMHPDidChange, stats);
    }

    private void OnDisable()
    {
        this.UnsubscribeFromSender<StatWillChangeEvent>(OnHPWillChange, stats);
        this.UnsubscribeFromSender<StatDidChangeEvent>(OnMHPDidChange, stats);
    }

    #endregion

    #region Event Handlers

    private void OnHPWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        e.Exception.AddModifier(new ClampValueModifier(int.MaxValue, MinHP, stats[StatTypes.MHP]));
    }

    private void OnMHPDidChange(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.MHP)
            return;

        var oldMHP = e.OldValue;
        if (MHP > oldMHP)
            HP += MHP - oldMHP;
        else
            HP = Mathf.Clamp(HP, MinHP, MHP);
    }

    #endregion
}