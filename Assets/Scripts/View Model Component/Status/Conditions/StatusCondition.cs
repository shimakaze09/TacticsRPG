using UnityEngine;

/// <summary>
/// Base expiry rule for a status: Remove() tells the Status registry to tear
/// the status down.
/// </summary>
public class StatusCondition : MonoBehaviour
{
    public virtual void Remove()
    {
        var s = GetComponentInParent<Status>();
        if (s)
            s.Remove(this);
    }
}