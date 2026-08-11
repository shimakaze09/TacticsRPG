using UnityEngine;

/// <summary>
/// Minimal component-based state machine: swaps State components, calling Exit
/// on the old and Enter on the new. Drives both battle flow and game flow.
/// Transition contract (issue #18): Exit/Enter run under try/finally so an
/// exception can never strand the machine mid-transition, and re-entrant
/// requests (a transition requested while one is running) are rejected loudly
/// instead of vanishing — callers that need queueing implement it above this
/// base (see GameFlowController).
/// </summary>
public class StateMachine : MonoBehaviour
{
    protected State _currentState;
    protected bool _inTransition;

    public virtual State CurrentState
    {
        get => _currentState;
        set => Transition(value);
    }

    public virtual T GetState<T>() where T : State
    {
        var target = GetComponent<T>() ?? gameObject.AddComponent<T>();
        return target;
    }

    public virtual void ChangeState<T>() where T : State
    {
        CurrentState = GetState<T>();
    }

    // Runs one Exit/Enter swap; the finally guarantees the machine is usable
    // again even when a state's Exit or Enter throws
    protected virtual void Transition(State value)
    {
        TryTransition(value);
    }

    /// <summary>
    /// Attempts the transition and reports whether it was applied — false for
    /// no-op (same state) and for rejected re-entrant requests. Flow layers
    /// use the result to keep their own bookkeeping atomic (issue #18).
    /// </summary>
    protected bool TryTransition(State value)
    {
        if (_currentState == value)
            return false;

        if (_inTransition)
        {
            Debug.LogWarning($"[StateMachine] Re-entrant transition to {(value != null ? value.GetType().Name : "null")} rejected while another transition is running on {name}.");
            return false;
        }

        _inTransition = true;

        try
        {
            if (_currentState != null)
                _currentState.Exit();

            _currentState = value;

            if (_currentState != null)
                _currentState.Enter();
        }
        finally
        {
            _inTransition = false;
        }

        return true;
    }
}
