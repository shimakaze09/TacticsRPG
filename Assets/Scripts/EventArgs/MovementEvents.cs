/// <summary>
/// Event arguments for movement system.
/// Handles unit movement validation and execution.
/// </summary>

/// <summary>
/// Raised to check if a unit can move.
/// Allows status effects to prevent movement.
/// </summary>
public class MovementCanMoveCheckEvent
{
    public Movement Movement { get; }
    public BaseException Exception { get; }

    public MovementCanMoveCheckEvent(Movement movement, BaseException exception)
    {
        Movement = movement;
        Exception = exception;
    }
}
