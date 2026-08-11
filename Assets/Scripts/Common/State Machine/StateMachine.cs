using System;
using UnityEngine;

/// <summary>
/// Outcome of a state-machine transition request (issue #18): flow layers
/// key their bookkeeping off this so logical state can never diverge from
/// machine state.
/// </summary>
public enum TransitionResult
{
    /// <summary>The swap ran and both lifecycle calls succeeded.</summary>
    Applied,

    /// <summary>The swap ran but Exit or Enter threw (logged, contained) — the machine IS on the new state.</summary>
    AppliedWithErrors,

    /// <summary>The requested state is already current; nothing ran.</summary>
    SameState,

    /// <summary>Rejected: another transition was already running.</summary>
    Rejected
}

/// <summary>
/// Minimal component-based state machine: swaps State components, calling Exit
/// on the old and Enter on the new. Drives both battle flow and game flow.
/// Transition contract (issue #18): the machine always finishes a swap on the
/// requested state — exceptions from Exit/Enter are logged and contained, and
/// the result reports them, so callers stay synchronized with machine reality
/// instead of unwinding into divergence. Re-entrant requests are rejected
/// loudly; layers that need queueing implement it above this base (see
/// GameFlowController).
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

    // Setter-path transition; layers that need the outcome call TryTransition
    protected virtual void Transition(State value)
    {
        TryTransition(value);
    }

    /// <summary>
    /// Runs one Exit/Enter swap and reports what happened. The machine ends
    /// on the requested state in every Applied* case, even when a lifecycle
    /// call throws — the exception is logged, never propagated, so no caller
    /// can end up half-transitioned (issue #18).
    /// </summary>
    protected TransitionResult TryTransition(State value)
    {
        if (_currentState == value)
            return TransitionResult.SameState;

        if (_inTransition)
        {
            Debug.LogWarning($"[StateMachine] Re-entrant transition to {(value != null ? value.GetType().Name : "null")} rejected while another transition is running on {name}.");
            return TransitionResult.Rejected;
        }

        _inTransition = true;
        var faulted = false;

        try
        {
            if (_currentState != null)
            {
                try
                {
                    _currentState.Exit();
                }
                catch (Exception ex)
                {
                    faulted = true;
                    Debug.LogException(ex, this);
                }
            }

            _currentState = value;

            if (_currentState != null)
            {
                try
                {
                    _currentState.Enter();
                }
                catch (Exception ex)
                {
                    faulted = true;
                    Debug.LogException(ex, this);
                }
            }
        }
        finally
        {
            _inTransition = false;
        }

        return faulted ? TransitionResult.AppliedWithErrors : TransitionResult.Applied;
    }
}
