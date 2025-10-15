using UnityEngine;


public class ProtectStatusEffect : StatusEffect
{
    private Stats myStats;

    // multiplier applied to DEF while Protect is active (e.g. 1.5 = +50% DEF)
    public float defMultiplier = 1.5f;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.AddObserver(OnStatWillChange, Stats.WillChangeNotification(StatTypes.DEF), myStats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnStatWillChange, Stats.WillChangeNotification(StatTypes.DEF), myStats);
    }


    private void OnStatWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        // multiply the final value
        var m = new MultValueModifier(0, defMultiplier);
        exc.AddModifier(m);
    }
}