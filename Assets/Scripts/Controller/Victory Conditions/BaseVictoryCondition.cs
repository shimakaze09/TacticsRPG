using System.Linq;
using UnityEngine;

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

    private void Awake()
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

    protected virtual bool IsDefeated(Unit unit)
    {
        var health = unit.GetComponent<Health>();
        if (health)
            return health.MinHP == health.HP;

        var stats = unit.GetComponent<Stats>();
        return stats[StatTypes.HP] == 0;
    }

    protected virtual bool PartyDefeated(Alliances type)
    {
        return !(from unit in bc.units
            let a = unit.GetComponent<Alliance>()
            where a != null
            where a.type == type && !IsDefeated(unit)
            select unit).Any();
    }

    protected virtual void CheckForGameOver()
    {
        if (PartyDefeated(Alliances.Hero))
            Victor = Alliances.Enemy;
    }

    #endregion
}