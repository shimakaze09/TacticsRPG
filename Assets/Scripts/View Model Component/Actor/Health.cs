using UnityEngine;
using System.Collections;

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
        this.AddObserver(OnHPWillChange, Stats.WillChangeNotification(StatTypes.HP), stats);
        this.AddObserver(OnMHPDidChange, Stats.DidChangeNotification(StatTypes.MHP), stats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnHPWillChange, Stats.WillChangeNotification(StatTypes.HP), stats);
        this.RemoveObserver(OnMHPDidChange, Stats.DidChangeNotification(StatTypes.MHP), stats);
    }

    #endregion

    #region Event Handlers

    private void OnHPWillChange(object sender, object args)
    {
        var vce = args as ValueChangeException;
        vce.AddModifier(new ClampValueModifier(int.MaxValue, MinHP, stats[StatTypes.MHP]));
    }

    private void OnMHPDidChange(object sender, object args)
    {
        var oldMHP = (int)args;
        if (MHP > oldMHP)
            HP += MHP - oldMHP;
        else
            HP = Mathf.Clamp(HP, MinHP, MHP);
    }

    #endregion
}