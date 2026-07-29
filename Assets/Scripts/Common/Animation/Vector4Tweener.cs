using UnityEngine;

/// <summary>
/// Tweener that interpolates a Vector4 between start and end values (base for
/// color tweening).
/// </summary>
public class Vector4Tweener : Tweener
{
    public Vector4 endTweenValue;
    public Vector4 startTweenValue;
    public Vector4 currentTweenValue { get; private set; }

    protected override void OnUpdate()
    {
        currentTweenValue = (endTweenValue - startTweenValue) * currentValue + startTweenValue;
        base.OnUpdate();
    }
}