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
            this.SubscribeToSender<StatWillChangeEvent>(OnStatWillChange, myStats);
    }

    private void OnDisable()
    {
        if (myStats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnStatWillChange, myStats);
    }


    private void OnStatWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.DEF)
            return;

        // multiply the final value
        var m = new MultValueModifier(0, defMultiplier);
        e.Exception.AddModifier(m);
    }
}