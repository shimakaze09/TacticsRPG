using UnityEngine;

public abstract class BehaviorOverrideEffect : MonoBehaviour
{
    protected Unit owner;

    protected virtual void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    protected virtual void OnDisable()
    {
        // Cleanup in subclasses
    }
}
