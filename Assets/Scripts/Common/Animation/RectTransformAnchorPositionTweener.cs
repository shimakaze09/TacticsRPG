using UnityEngine;

/// <summary>
/// Tweener that animates a RectTransform's anchoredPosition (UI panel
/// slide-in/out).
/// </summary>
public class RectTransformAnchorPositionTweener : Vector3Tweener
{
    private RectTransform rt;

    private void Awake()
    {
        rt = transform as RectTransform;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        rt.anchoredPosition = currentTweenValue;
    }
}