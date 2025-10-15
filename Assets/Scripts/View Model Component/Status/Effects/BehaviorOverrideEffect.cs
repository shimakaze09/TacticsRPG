using UnityEngine;

public abstract class BehaviorOverrideEffect : MonoBehaviour
{
    protected Unit owner;

    protected virtual void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        // Example: subscribe to a hypothetical AI target selection event if present in your project.
        // this.AddObserver(OnAiSelectTarget, AiController.SelectTargetNotification, owner);
    }

    protected virtual void OnDisable()
    {
        // Unsubscribe if you subscribe in subclasses
    }

    // Subclasses should implement this to alter or replace target selection
    // public abstract void OnAiSelectTarget(object sender, object args);
}