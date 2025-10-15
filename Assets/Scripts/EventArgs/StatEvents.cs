/// <summary>
/// Event arguments for stat modification system.
/// Supports both pre-modification (WillChange) and post-modification (DidChange) events.
/// </summary>

/// <summary>
/// Raised before a stat value changes.
/// Allows modifiers to adjust the change or cancel it entirely.
/// </summary>
public class StatWillChangeEvent
{
    public Stats Stats { get; }
    public StatTypes StatType { get; }
    public ValueChangeException Exception { get; }

    public StatWillChangeEvent(Stats stats, StatTypes statType, ValueChangeException exception)
    {
        Stats = stats;
        StatType = statType;
        Exception = exception;
    }
}

/// <summary>
/// Raised after a stat value has changed.
/// Contains the old value for comparison.
/// </summary>
public class StatDidChangeEvent
{
    public Stats Stats { get; }
    public StatTypes StatType { get; }
    public int OldValue { get; }
    public int NewValue { get; }

    public StatDidChangeEvent(Stats stats, StatTypes statType, int oldValue, int newValue)
    {
        Stats = stats;
        StatType = statType;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
