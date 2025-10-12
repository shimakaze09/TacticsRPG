public class SlowStatusEffect : StatusEffect
{
    private Stats myStats;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.AddObserver(OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats);
    }

    private void OnCounterWillChange(object sender, object args)
    {
        var exc = args as ValueChangeException;
        var m = new MultDeltaModifier(0, 0.5f);
        exc.AddModifier(m);
    }
}