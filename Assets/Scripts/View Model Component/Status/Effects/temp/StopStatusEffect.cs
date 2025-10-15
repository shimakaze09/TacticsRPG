public class StopStatusEffect : StatusEffect
{
    private Stats myStats;

    private void OnEnable()
    {
        myStats = GetComponentInParent<Stats>();
        if (myStats)
            this.SubscribeToSender<StatWillChangeEvent>(OnCounterWillChange, myStats);
        this.Subscribe<AutomaticHitCheckEvent>(OnAutomaticHitCheck);
    }

    private void OnDisable()
    {
        if (myStats != null)
            this.UnsubscribeFromSender<StatWillChangeEvent>(OnCounterWillChange, myStats);
        this.Unsubscribe<AutomaticHitCheckEvent>(OnAutomaticHitCheck);
    }

    private void OnCounterWillChange(StatWillChangeEvent e)
    {
        if (e.StatType != StatTypes.CTR)
            return;

        e.Exception.FlipToggle();
    }

    private void OnAutomaticHitCheck(AutomaticHitCheckEvent e)
    {
        var owner = GetComponentInParent<Unit>();
        if (owner == e.Target)
            e.Exception.FlipToggle();
    }
}