/// <summary>
/// Tweener that animates a Transform's localEulerAngles.
/// </summary>
public class TransformLocalEulerTweener : Vector3Tweener
{
    protected override void OnUpdate()
    {
        base.OnUpdate();
        transform.localEulerAngles = currentTweenValue;
    }
}