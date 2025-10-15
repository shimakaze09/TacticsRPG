public class HasteStatusEffect : StatusEffect
{
    private Stats myStats;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.SubscribeToSender<StatWillChangeEvent>(OnCounterWillChange, myStats);
    }

    private void OnDisable()
    {
        if (myStats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnCounterWillChange, myStats);
    }

    private void OnCounterWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.CTR)
            return;

        var m = new MultDeltaModifier(0, 2);
        e.Exception.AddModifier(m);
    }
}