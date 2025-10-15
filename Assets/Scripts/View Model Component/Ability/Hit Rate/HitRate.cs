using UnityEngine;

public abstract class HitRate : MonoBehaviour
{
    #region MonoBehaviour

    protected virtual void Start()
    {
        attacker = GetComponentInParent<Unit>();
    }

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
        this.Publish(new AutomaticHitCheckEvent(attacker, target, exc));
        return exc.toggle;
    }

    protected virtual bool AutomaticMiss(Unit target)
    {
        var exc = new MatchException(attacker, target);
        this.Publish(new AutomaticMissCheckEvent(attacker, target, exc));
        return exc.toggle;
    }

    protected virtual int AdjustForStatusEffects(Unit target, int rate)
    {
        var args = new HitRateStatusCheckArgs(rate);
        this.Publish(new HitRateStatusCheckEvent(attacker, target, args));
        return args.HitRate;
    }

    protected virtual int Final(int evade)
    {
        return accuracy - evade;
    }

    #endregion
}