public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private void OnEnable()
    {
        this.Subscribe<TurnBeganEvent>(OnNewTurn);
    }

    private void OnDisable()
    {
        this.Unsubscribe<TurnBeganEvent>(OnNewTurn);
    }

    private void OnNewTurn(TurnBeganEvent e)
    {
        duration--;
        if (duration <= 0)
            Remove();
    }
}