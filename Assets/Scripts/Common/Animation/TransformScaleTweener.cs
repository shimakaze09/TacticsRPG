/// <summary>
/// Tweener that animates a Transform's localScale.
/// </summary>
public class TransformScaleTweener : Vector3Tweener
{
    protected override void OnUpdate()
    {
        base.OnUpdate();
        transform.localScale = currentTweenValue;
    }
}