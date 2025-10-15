/// <summary>
/// Event arguments for hit rate calculation system.
/// Manages hit/miss determination for attacks.
/// </summary>

/// <summary>
/// Raised to check if an attack should automatically hit.
/// Used by status effects like Stop to force hits.
/// </summary>
public class AutomaticHitCheckEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public MatchException Exception { get; }

    public AutomaticHitCheckEvent(Unit attacker, Unit target, MatchException exception)
    {
        Attacker = attacker;
        Target = target;
        Exception = exception;
    }
}

/// <summary>
/// Raised to check if an attack should automatically miss.
/// Used by status effects to force misses.
/// </summary>
public class AutomaticMissCheckEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public MatchException Exception { get; }

    public AutomaticMissCheckEvent(Unit attacker, Unit target, MatchException exception)
    {
        Attacker = attacker;
        Target = target;
        Exception = exception;
    }
}

/// <summary>
/// Raised to check for status-based hit rate modifications.
/// Used by status effects like Blind to modify hit chance.
/// </summary>
public class HitRateStatusCheckEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public HitRateStatusCheckArgs Args { get; }

    public HitRateStatusCheckEvent(Unit attacker, Unit target, HitRateStatusCheckArgs args)
    {
        Attacker = attacker;
        Target = target;
        Args = args;
    }
}

/// <summary>
/// Arguments for hit rate status check containing the modifiable hit rate
/// </summary>
public class HitRateStatusCheckArgs
{
    public int HitRate { get; set; }

    public HitRateStatusCheckArgs(int hitRate)
    {
        HitRate = hitRate;
    }
}
