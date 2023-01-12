using UnityEngine;
using System.Collections;

public class ReviveAbilityEffect : BaseAbilityEffect
{
    public float percent;

    public override int Predict(Tile target)
    {
        var s = target.content.GetComponent<Stats>();
        return Mathf.FloorToInt(s[StatTypes.MHP] * percent);
    }

    protected override int OnApply(Tile target)
    {
        var s = target.content.GetComponent<Stats>();
        var value = s[StatTypes.HP] = Predict(target);
        return value;
    }
}