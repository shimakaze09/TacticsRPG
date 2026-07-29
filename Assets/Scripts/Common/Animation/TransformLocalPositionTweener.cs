/// <summary>
/// Tweener that animates a Transform's localPosition.
/// </summary>
public class TransformLocalPositionTweener : Vector3Tweener
{
    protected override void OnUpdate()
    {
        base.OnUpdate();
        transform.localPosition = currentTweenValue;
    }
}