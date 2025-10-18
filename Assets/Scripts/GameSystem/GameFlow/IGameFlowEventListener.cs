/// <summary>
/// Interface for components that need to respond to game flow state changes.
/// Allows decoupled communication between GameFlowController and UI/gameplay systems.
/// </summary>
public interface IGameFlowEventListener
{
    /// <summary>
    /// Called when the game flow state is about to change.
    /// </summary>
    /// <param name="fromState">The state we're leaving</param>
    /// <param name="toState">The state we're entering</param>
    void OnFlowStateChanging(GameFlowState fromState, GameFlowState toState);

    /// <summary>
    /// Called after the game flow state has changed and the new state has been entered.
    /// </summary>
    /// <param name="newState">The newly active state</param>
    void OnFlowStateChanged(GameFlowState newState);
}
