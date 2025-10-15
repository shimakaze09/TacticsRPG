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
            this.AddObserver(OnStatWillChange, Stats.WillChangeNotification(StatTypes.MDF), myStats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnStatWillChange, Stats.WillChangeNotification(StatTypes.MDF), myStats);
    }

    private void OnStatWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        var m = new MultValueModifier(0, mdfMultiplier);
        exc.AddModifier(m);
    }
}