using UnityEngine;

public class IndicatorAnimator : MonoBehaviour
{
    private void Start()
    {
        var scale = gameObject.transform.localScale;
        var t = gameObject.transform.ScaleTo(new Vector3(scale.x - 0.05f, scale.y, scale.z - 0.05f), 0.5f,
            EasingEquations.EaseInQuad);
        t.loopType = EasingControl.LoopType.PingPong;
        t.loopCount = -1;
    }
}