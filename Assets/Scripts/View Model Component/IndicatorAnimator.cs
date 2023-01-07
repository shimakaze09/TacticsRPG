using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class IndicatorAnimator : MonoBehaviour
{
    private void Start()
    {
        var scale = gameObject.transform.localScale;
        var t = gameObject.transform.ScaleTo(new Vector3(scale.x - 0.05f, scale.y, scale.z - 0.05f), 0.5f,
            EasingEquations.EaseInQuad);
        t.easingControl.loopType = EasingControl.LoopType.PingPong;
        t.easingControl.loopCount = -1;
    }
}