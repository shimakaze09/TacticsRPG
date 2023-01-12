using UnityEngine;
using System.Collections;

public class StatusCondition : MonoBehaviour
{
    public virtual void Remove()
    {
        var s = GetComponentInParent<Status>();
        if (s)
            s.Remove(this);
    }
}