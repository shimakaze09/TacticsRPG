using UnityEngine;

/// <summary>
/// Removes its status after N of the owner's turns. Units denied their turns
/// (CT frozen by Blackout/FreezeFrame/Graycast) would otherwise never tick,
/// making "3 turns" mean forever — the #12 failure — so a battle-round
/// fallback also decrements once for every full round in which the owner
/// never activated. Statuses therefore expire in N turns' worth of battle
/// time whether or not the victim ever gets to act (issue #57).
/// </summary>
public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private Unit owner;
    private BattleClock clock;
    private int lastRound;
    private bool ownerTickedThisRound;

    private void OnEnable()
    {
        // Tick only on the afflicted unit's own turns — a global subscription
        // would burn "3 turns" in half a round of a 6-unit battle.
        owner = GetComponentInParent<Unit>();
        if (owner != null)
            this.SubscribeToSender<TurnBeganEvent>(OnNewTurn, owner);
        else
            this.Subscribe<TurnBeganEvent>(OnNewTurn);

        // Round fallback for owners who never reach a turn
        this.Subscribe<TurnCompletedEvent>(OnAnyTurnCompleted);
        clock = FindAnyObjectByType<BattleClock>();
        lastRound = clock != null ? clock.CurrentRound : 0;
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
        ownerTickedThisRound = true;
        Tick();
    }

    // When a battle round rolls over without the owner having activated, the
    // owner was denied its turn — decrement anyway so frozen units' statuses
    // (and their Steeled protection) still expire
    private void OnAnyTurnCompleted(TurnCompletedEvent e)
    {
        if (clock == null || clock.CurrentRound == lastRound)
            return;

        lastRound = clock.CurrentRound;
        bool ticked = ownerTickedThisRound;
        ownerTickedThisRound = false;
        if (!ticked)
            Tick();
    }

    // Shared decrement-and-expire step for both tick sources
    private void Tick()
    {
        duration--;
        if (duration <= 0)
            Remove();
    }
}
