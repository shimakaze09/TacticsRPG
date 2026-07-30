/// <summary>
/// Victory when the party is still standing after N battle rounds
/// (round definition: BattleClock). Defeat rules from the base still apply.
/// </summary>
public class SurviveRoundsVictoryCondition : BaseVictoryCondition
{
    public int rounds = 6;

    private BattleClock clock;

    // Checks the clock after every completed activation
    protected override void OnEnable()
    {
        base.OnEnable();
        this.Subscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.Unsubscribe<TurnCompletedEvent>(OnTurnCompleted);
    }

    private void OnTurnCompleted(TurnCompletedEvent e)
    {
        if (Victor != Alliances.None)
            return;

        if (clock == null)
            clock = GetComponent<BattleClock>();
        if (clock == null)
            return;

        if (clock.CurrentRound > rounds && !PartyDefeated(Alliances.Hero))
            Victor = Alliances.Hero;
    }
}
