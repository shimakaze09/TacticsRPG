using System;

/// <summary>
/// Base class for value tweeners: an EasingControl that optionally destroys
/// itself when the animation completes.
/// </summary>
public abstract class Tweener : EasingControl
{
    #region Event Handlers

    protected override void OnComplete()
    {
        base.OnComplete();
        if (destroyOnComplete)
            Destroy(this);
    }

    #endregion

    #region Properties

    public static float DefaultDuration = 1f;
    public static Func<float, float, float, float> DefaultEquation = EasingEquations.EaseInOutQuad;
    public bool destroyOnComplete = true;

    #endregion
}