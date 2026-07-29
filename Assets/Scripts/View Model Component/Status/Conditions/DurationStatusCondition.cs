public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private Unit owner;

    private void OnEnable()
    {
        // Tick only on the afflicted unit's own turns — a global subscription
        // would burn "3 turns" in half a round of a 6-unit battle.
        owner = GetComponentInParent<Unit>();
        if (owner != null)
            this.SubscribeToSender<TurnBeganEvent>(OnNewTurn, owner);
        else
            this.Subscribe<TurnBeganEvent>(OnNewTurn);
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnNewTurn, owner);
        else
            this.Unsubscribe<TurnBeganEvent>(OnNewTurn);
    }

    private void OnNewTurn(TurnBeganEvent e)
    {
        duration--;
        if (duration <= 0)
            Remove();
    }
}
