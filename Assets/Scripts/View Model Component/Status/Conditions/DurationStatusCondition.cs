using UnityEngine;

/// <summary>
/// Removes its status after N of the owner's turns. Owners denied all turns
/// (CT frozen by an ICtFreezingStatus) never tick naturally — the #12
/// failure — so StatusExpiryRules drives FallbackActivation battle-wide: one
/// full round-length window of activations without an owner turn counts as
/// one denied turn. The rules component snapshots frozen-ness per unit
/// before ticking, so a control expiring mid-event still counts that window
/// for its sibling conditions; scoping to actual CT freeze keeps naturally
/// slow units' statuses at full duration, and per-condition windows mean a
/// late inflict always gets its complete first window (issue #57).
/// </summary>
public class DurationStatusCondition : StatusCondition
{
    public int duration = 10;

    private Unit owner;
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
        activationsSinceTick = 0;
        Tick();
    }

    /// <summary>
    /// Advances the frozen-window fallback clock. Called by StatusExpiryRules
    /// once per completed battle activation, with the owner's frozen state
    /// snapshotted before any of its conditions tick: a full round-length
    /// window while frozen equals one denied turn; any unfrozen activation
    /// resets the window.
    /// </summary>
    public void FallbackActivation(bool ownerFrozen, int roundLength)
    {
        if (!ownerFrozen)
        {
            activationsSinceTick = 0;
            return;
        }

        activationsSinceTick++;
        if (activationsSinceTick >= roundLength)
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
