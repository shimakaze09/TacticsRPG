using System.Linq;
using UnityEngine;

public abstract class BaseVictoryCondition : MonoBehaviour
{
    #region Notification Handlers

    protected virtual void OnHPDidChangeNotification(object sender, object args)
    {
        CheckForGameOver();
    }

    #endregion

    #region Fields & Properties

    public Alliances Victor { get; protected set; } = Alliances.None;

    protected BattleController bc;

    #endregion

    #region MonoBehaviour

    protected virtual void Awake()
    {
        bc = GetComponent<BattleController>();
    }

    protected virtual void OnEnable()
    {
        this.AddObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
    }

    protected virtual void OnDisable()
    {
        this.RemoveObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
    }

    #endregion

    #region Protected

    protected virtual void CheckForGameOver()
    {
        if (PartyDefeated(Alliances.Hero))
            Victor = Alliances.Enemy;
    }

    protected virtual bool PartyDefeated(Alliances type)
    {
        return !(from t in bc.units
            let a = t.GetComponent<Alliance>()
            where a != null
            where a.type == type && !IsDefeated(t)
            select t).Any();
    }

    protected virtual bool IsDefeated(Unit unit)
    {
        var health = unit.GetComponent<Health>();
        if (health)
            return health.MinHP == health.HP;

        var stats = unit.GetComponent<Stats>();
        return stats[StatTypes.HP] == 0;
    }

    #endregion
}