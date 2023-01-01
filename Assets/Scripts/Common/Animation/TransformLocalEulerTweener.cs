using System;

public class TransformLocalEulerTweener : Vector3Tweener
{
    protected override void OnUpdate(object sender, EventArgs e)
    {
        base.OnUpdate(sender, e);
        transform.localEulerAngles = currentValue;
    }
}