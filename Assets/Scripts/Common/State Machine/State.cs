using UnityEngine;

/// <summary>
/// Base class for states used by StateMachine: Enter/Exit plus event-listener
/// setup, one state active at a time.
/// </summary>
public abstract class State : MonoBehaviour
{
    public virtual void Enter()
    {
        AddListeners();
    }

    public virtual void Exit()
    {
        RemoveListeners();
    }

    protected virtual void OnDestroy()
    {
        RemoveListeners();
    }

    protected virtual void AddListeners()
    {
    }

    protected virtual void RemoveListeners()
    {
    }
}