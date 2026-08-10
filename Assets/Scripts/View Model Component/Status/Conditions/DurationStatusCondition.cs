using UnityEngine;

/// <summary>
/// Removes its status after N of the owner's turns. Owners denied all turns
/// (CT frozen by a status implementing ICtFreezingStatus) would never tick,
/// making "3 turns" mean forever — the #12 failure — so while the owner is
/// frozen the condition also counts battle-wide activations and ticks once
/// per full round-length window measured from its last tick. Scoping the
/// fallback to actual CT freeze keeps naturally slow units' statuses at full
/// duration, and measuring the window from the last tick (not from round
/// boundaries) means a late-round inflict still gets its full first window
/// (issue #57).
/// </summary>
public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private Unit owner;
    private BattleClock clock;
    private int activationsSinceTick;

    private void OnEnable()
    {
        // Tick only on the afflicted unit's own turns — a global subscription
        // would burn "3 turns" in half a round of a 6-unit battle.
        owner = GetComponentInParent<Unit>();
        if (owner != null)
            this.SubscribeToSender<TurnBeganEvent>(OnNewTurn, owner);
        else
            this.Subscribe<TurnBeganEvent>(OnNewTurn);

        // Fallback clock for owners whose CT is frozen
        this.Subscribe<TurnCompletedEvent>(OnAnyTurnCompleted);
        clock = FindAnyObjectByType<BattleClock>();
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnBeganEvent>(OnNewTurn, owner);
        else
            this.Unsubscribe<TurnBeganEvent>(OnNewTurn);

        this.Unsubscribe<TurnCompletedEvent>(OnAnyTurnCompleted);
    }

    private void OnNewTurn(TurnBeganEvent e)
    {
        activationsSinceTick = 0;
        Tick();
    }

    // While the owner is CT-frozen, count every battle activation; one full
    // round-length window without an owner turn equals one denied turn.
    // Unfrozen owners never fallback-tick — their own turns are the clock.
    private void OnAnyTurnCompleted(TurnCompletedEvent e)
    {
        if (clock == null || owner == null)
            return;

        if (owner.GetComponentInChildren<ICtFreezingStatus>() == null)
        {
            activationsSinceTick = 0;
            return;
        }

        activationsSinceTick++;
        if (activationsSinceTick >= clock.RoundLength)
        {
            activationsSinceTick = 0;
            Tick();
        }
    }

    // Shared decrement-and-expire step for both tick sources
    private void Tick()
    {
        duration--;
        if (duration <= 0)
            Remove();
    }
}
