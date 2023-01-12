using UnityEngine;
using System.Collections;

public abstract class BaseVictoryCondition : MonoBehaviour
{
    #region Fields & Properties

    public Alliances Victor
    {
        get => victor;
        protected set => victor = value;
    }

    private Alliances victor = Alliances.None;

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

    #region Notification Handlers

    protected virtual void OnHPDidChangeNotification(object sender, object args)
    {
        CheckForGameOver();
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
        for (var i = 0; i < bc.units.Count; ++i)
        {
            var a = bc.units[i].GetComponent<Alliance>();
            if (a == null)
                continue;

            if (a.type == type && !IsDefeated(bc.units[i]))
                return false;
        }

        return true;
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