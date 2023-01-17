using System.Collections;
using System;
using UnityEngine;
using TMPro;

public static class TextMeshProAnimationExtension
{
    public static Tweener ChangeColor(this TextMeshProUGUI t, Color32 change)
    {
        return ChangeColor(t, change, Tweener.DefaultDuration);
    }

    public static Tweener ChangeColor(this TextMeshProUGUI t, Color32 change, float duration)
    {
        return ChangeColor(t, change, duration, Tweener.DefaultEquation);
    }

    public static Tweener ChangeColor(this TextMeshProUGUI t, Color32 change, float duration,
        Func<float, float, float, float> equation)
    {
        ColorTweener tweener = t.gameObject.AddComponent<ColorTweener>();
        Color color;

        tweener.startTweenValue = t.color;
        color = change;
        tweener.endTweenValue = color;
        tweener.duration = duration;
        tweener.equation = equation;
        tweener.Play();
        return tweener;
    }
}