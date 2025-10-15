using UnityEngine;

public class ReactionEffect : MonoBehaviour
{
    // This class intentionally provides no concrete behavior. It is a convenience base for subscription and cleanup.
    protected Unit owner;

    protected virtual void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
    }

    protected virtual void OnDisable()
    {
        // subclasses should remove observers they added
    }

    // Example helper: remove the StatusCondition
    protected void RemoveCondition()
    {
        var cond = GetComponentInChildren<StatusCondition>() ?? GetComponent<StatusCondition>();
        if (cond != null)
            cond.Remove();
    }
}