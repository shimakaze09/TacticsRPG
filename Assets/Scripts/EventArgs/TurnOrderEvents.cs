/// <summary>
/// Event arguments for turn order system events.
/// These events manage the turn-based combat flow.
/// </summary>

/// <summary>
/// Raised at the beginning of each combat round.
/// All units increment their CTR counters based on SPD.
/// </summary>
public class RoundBeganEvent
{
    public RoundBeganEvent() { }
}

/// <summary>
/// Raised to check if a unit can take their turn.
/// Allows status effects to prevent turn activation.
/// </summary>
public class TurnCheckEvent
{
    public Unit Unit { get; }
    public BaseException Exception { get; }

    public TurnCheckEvent(Unit unit, BaseException exception)
    {
        Unit = unit;
        Exception = exception;
    }
}

/// <summary>
/// Raised when a unit's turn begins.
/// Used by periodic effects, countdown timers, etc.
/// </summary>
public class TurnBeganEvent
{
    public Unit Unit { get; }

    public TurnBeganEvent(Unit unit)
    {
        Unit = unit;
    }
}

/// <summary>
/// Raised when a unit's turn is completed.
/// CTR costs have been deducted.
/// </summary>
public class TurnCompletedEvent
{
    public Unit Unit { get; }

    public TurnCompletedEvent(Unit unit)
    {
        Unit = unit;
    }
}

/// <summary>
/// Raised at the end of each combat round.
/// All eligible units have taken their turns.
/// </summary>
public class RoundEndedEvent
{
    public RoundEndedEvent() { }
}
