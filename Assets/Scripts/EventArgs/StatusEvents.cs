/// <summary>
/// Event arguments for status effect system.
/// Manages application and removal of status effects.
/// </summary>

/// <summary>
/// Raised when a status effect is added to a unit.
/// </summary>
public class StatusEffectAddedEvent
{
    public Status Status { get; }
    public StatusEffect Effect { get; }

    public StatusEffectAddedEvent(Status status, StatusEffect effect)
    {
        Status = status;
        Effect = effect;
    }
}

/// <summary>
/// Raised when a status effect is removed from a unit.
/// </summary>
public class StatusEffectRemovedEvent
{
    public Status Status { get; }
    public StatusEffect Effect { get; }

    public StatusEffectRemovedEvent(Status status, StatusEffect effect)
    {
        Status = status;
        Effect = effect;
    }
}
