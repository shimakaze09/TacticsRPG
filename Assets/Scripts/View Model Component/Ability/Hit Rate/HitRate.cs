using UnityEngine;

/// <summary>
/// Base accuracy calculator: rolls to-hit from a 0-100 chance, with hooks for
/// auto-hit/miss and status adjustments (Static, Ghosted).
/// </summary>
public abstract class HitRate : MonoBehaviour
{
    #region Fields

    public virtual bool IsAngleBased => true;
    public int accuracy = 100;

    private Unit cachedAttacker;

    /// <summary>
    /// The unit performing the ability. Resolved lazily because ability
    /// prefabs are parented to their unit after instantiation — a Start()
    /// cache is null for anything used the frame its owner spawns.
    /// </summary>
    protected Unit attacker
    {
        get
        {
            if (cachedAttacker == null)
                cachedAttacker = GetComponentInParent<Unit>();
            return cachedAttacker;
        }
    }

    #endregion

    #region Public

    /// <summary>
    ///     Returns a value in the range of 0 t0 100 as a percent chance of
    ///     an ability succeeding to hit
    /// </summary>
    public abstract int Calculate(Tile target);

    public virtual bool RollForHit(Tile target)
    {
        // roll in [0, 99]: chance 0 never hits, chance 100 always hits
        var roll = Random.Range(0, 100);
        var chance = Calculate(target);
        return roll < chance;
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