using UnityEngine;

public abstract class HitRate : MonoBehaviour
{
    #region MonoBehaviour

    protected virtual void Start()
    {
        attacker = GetComponentInParent<Unit>();
    }

    #endregion

    #region Notifications

    /// <summary>
    ///     Includes a toggleable MatchException argument which defaults to false.
    /// </summary>
    public const string AutomaticHitCheckNotification = "HitRate.AutomaticHitCheckNotification";

    /// <summary>
    ///     Includes a toggleable MatchException argument which defaults to false.
    /// </summary>
    public const string AutomaticMissCheckNotification = "HitRate.AutomaticMissCheckNotification";

    /// <summary>
    ///     Includes an Info argument with three parameters: Attacker (Unit), Defender (Unit),
    ///     and Defender's calculated Evade / Resistance (int).  Status effects which modify Hit Rate
    ///     should modify the arg2 parameter.
    /// </summary>
    public const string StatusCheckNotification = "HitRate.StatusCheckNotification";

    #endregion

    #region Fields

    public virtual bool IsAngleBased => true;
    public int accuracy = 100;
    protected Unit attacker;

    #endregion

    #region Public

    /// <summary>
    ///     Returns a value in the range of 0 t0 100 as a percent chance of
    ///     an ability succeeding to hit
    /// </summary>
    public abstract int Calculate(Tile target);

    public virtual bool RollForHit(Tile target)
    {
        var roll = Random.Range(0, 101);
        var chance = Calculate(target);
        return roll <= chance;
    }

    #endregion

    #region Protected

    protected virtual bool AutomaticHit(Unit target)
    {
        var exc = new MatchException(attacker, target);
        this.PostNotification(AutomaticHitCheckNotification, exc);
        return exc.toggle;
    }

    protected virtual bool AutomaticMiss(Unit target)
    {
        var exc = new MatchException(attacker, target);
        this.PostNotification(AutomaticMissCheckNotification, exc);
        return exc.toggle;
    }

    protected virtual int AdjustForStatusEffects(Unit target, int rate)
    {
        var args = new Info<Unit, Unit, int>(attacker, target, rate);
        this.PostNotification(StatusCheckNotification, args);
        return args.arg2;
    }

    protected virtual int Final(int evade)
    {
        return accuracy - evade;
    }

    #endregion
}