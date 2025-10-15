using UnityEngine;


public class ShellStatusEffect : StatusEffect
{
    private Stats myStats;

    // multiplier applied to MDF while Shell is active (e.g. 1.5 = +50% MDF)
    public float mdfMultiplier = 1.5f;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, myStats);
    }

    private void OnDisable()
    {
        if (myStats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, myStats);
    }

    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.MDF)
            return;

        var m = new MultValueModifier(0, mdfMultiplier);
        e.Exception.AddModifier(m);
    }
}